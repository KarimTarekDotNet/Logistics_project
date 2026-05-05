using Application.ApplicationRules.Shipments;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentService : IShipmentService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ShipmentResponse?> ChangeStatusAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if(user == null || !isPrivileged)
            {
                throw new BusinessRuleException("User not found or does not have permission to change shipment status.");
            }
            else if (user == null || user.CustomerProfile == null)
                return null;
            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
            if(shipment == null)
                return null;

            var oldStatus = shipment.Status;
            var newStatus = request.ToStatus;

            if (!ShipmentStatusRules.CanTransition(oldStatus, newStatus))
                throw new BusinessRuleException($"Cannot change shipment status from {oldStatus} to {newStatus}.");


            shipment.Status = newStatus;
            shipment.UpdatedAt = DateTimeOffset.UtcNow;

            switch (newStatus)
            {
                case ShipmentStatus.ClientConfirmed:
                    shipment.ClientConfirmedAt = DateTimeOffset.UtcNow;
                    break;

                case ShipmentStatus.BookingRequested:
                    shipment.BookingRequestedAt = DateTimeOffset.UtcNow;
                    break;

                case ShipmentStatus.BookingConfirmed:
                    shipment.BookingConfirmedAt = DateTimeOffset.UtcNow;
                    break;

                case ShipmentStatus.Delivered:
                    shipment.DeliveredAt = DateTimeOffset.UtcNow;
                    break;
            }

            shipment.StatusHistory.Add(new ShipmentStatusHistory
            {
                ShipmentId = shipment.Id,
                FromStatus = oldStatus,
                ToStatus = newStatus,
                ChangedAt = DateTimeOffset.UtcNow,
                ChangedBy = user.UserName,
                Reason = request.Reason
            });

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentResponse>(shipment);
        }

        public async Task<ShipmentResponse> CreateAsync(string userId, CreateShipmentRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                throw new BusinessRuleException("User not found or does not have a customer profile.");

            var quote = await _unitOfWork.Quotes.GetByIdAsync(request.QuoteId);
            if (quote == null || quote.CustomerId != user.CustomerProfile.Id)
                throw new BusinessRuleException("Quote not found.");

            var carrier = await _unitOfWork.Carriers.GetByIdAsync(request.CarrierId);
            if (carrier == null)
                throw new BusinessRuleException("Carrier not found.");

            var hasValidRate = await _unitOfWork.Rates.ExistsActiveRateAsync(request.CarrierId,
            quote.RouteId, quote.ContainerTypeId);

            if (!hasValidRate)
                throw new BusinessRuleException("Carrier does not have an active rate for this quote route/container.");

            var existingShipment = await _unitOfWork.Shipments.ExistsByQuoteIdAsync(request.QuoteId);
            if (existingShipment)
                throw new BusinessRuleException("This quote is already used.");

            var shipment = new Shipment
            {

                QuoteId = quote.Id,
                CustomerId = quote.CustomerId,
                RouteId = quote.RouteId,
                ContainerTypeId = quote.ContainerTypeId,
                CarrierId = request.CarrierId,
                AgreedPrice = quote.FinalPrice,
                Currency = quote.Currency,

                Status = ShipmentStatus.Created,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.Shipments.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentResponse>(shipment);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                return false;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
            if (shipment == null || shipment.CustomerId != user.CustomerProfile.Id)
                return false;

            if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                throw new BusinessRuleException("Cannot delete delivered shipment.");

            _unitOfWork.Shipments.Delete(shipment);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(ShipmentParameters parameters)
        {
            var shipments = await _unitOfWork.Shipments.GetAllAsync(parameters);
            if (shipments == null || shipments.Count == 0)
                return new List<ShipmentResponse>();
            return _mapper.Map<IReadOnlyList<ShipmentResponse>>(shipments);
        }

        public async Task<IReadOnlyList<ShipmentResponse>> GetAllForUserAsync(string userId, ShipmentParameters parameters)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                return new List<ShipmentResponse>();

            var shipments = await _unitOfWork.Shipments.GetAllForUserAsync(user.CustomerProfile.Id, parameters);
            if (shipments == null || shipments.Count == 0)
                return new List<ShipmentResponse>();

            return _mapper.Map<IReadOnlyList<ShipmentResponse>>(shipments);
        }

        public async Task<ShipmentResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            var shipment = await _unitOfWork.Shipments.GetByIdWithDetailsAsync(id);
            if (shipment == null)
                return null;

            if (isPrivileged)
                return _mapper.Map<ShipmentResponse>(shipment);

            if (user.CustomerProfile == null)
                return null;

            if (shipment.CustomerId != user.CustomerProfile.Id)
                return null;

            return _mapper.Map<ShipmentResponse>(shipment);
        }

        public async Task<ShipmentResponse?> UpdateAsync(Guid id, string userId, UpdateShipmentRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                return null;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
            if (shipment == null || shipment.CustomerId != user.CustomerProfile.Id)
                return null;

            if (shipment.Status == ShipmentStatus.Delivered)
                throw new BusinessRuleException("Cannot update delivered shipment.");

            if (!request.QuoteId.HasValue)
                request.QuoteId = shipment.QuoteId;

            if (!request.CarrierId.HasValue)
                request.CarrierId = shipment.CarrierId;

            var quote = await _unitOfWork.Quotes.GetByIdAsync(request.QuoteId.Value);
            if (quote == null || quote.CustomerId != user.CustomerProfile.Id)
                throw new BusinessRuleException("Quote not found.");

            var carrier = await _unitOfWork.Carriers.GetByIdAsync(request.CarrierId.Value);
            if (carrier == null)
                throw new BusinessRuleException("Carrier not found.");

            var hasValidRate = await _unitOfWork.Rates.ExistsActiveRateAsync(request.CarrierId.Value,
            quote.RouteId, quote.ContainerTypeId);

            if (!hasValidRate)
                throw new BusinessRuleException("Carrier does not have an active rate for this quote route/container.");

            var existingShipment = await _unitOfWork.Shipments
                .ExistsByQuoteIdExceptAsync(request.QuoteId.Value, shipment.Id);

            if (existingShipment)
                throw new BusinessRuleException("This quote is already used.");

            shipment.QuoteId = request.QuoteId.Value;
            shipment.CarrierId = request.CarrierId.Value;

            shipment.CustomerId = quote.CustomerId;
            shipment.RouteId = quote.RouteId;
            shipment.ContainerTypeId = quote.ContainerTypeId;
            shipment.AgreedPrice = quote.FinalPrice;
            shipment.Currency = quote.Currency;
            shipment.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentResponse>(shipment);
        }
    }
}
