using Application.ApplicationRules.Shipments;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentCommandService : IShipmentCommandService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentCommandService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentResponse> CreateAsync(string userId, CreateShipmentRequest request)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null || user.CustomerProfile == null)
                    throw new BusinessRuleException("User not found or does not have a customer profile.");

                var quote = await _unitOfWork.Quotes.GetByIdAsync(request.QuoteId);
                if (quote == null || quote.CustomerId != user.CustomerProfile.Id)
                    throw new BusinessRuleException("Quote not found.");

                var carrier = await _unitOfWork.Carriers.GetByIdAsync(quote.CarrierId);
                if (carrier == null)
                    throw new BusinessRuleException("Carrier not found.");

                var hasValidRate = await _unitOfWork.Rates.ExistsActiveRateAsync(quote.CarrierId,
                quote.RouteId, quote.ContainerTypeId);

                if (!hasValidRate)
                    throw new BusinessRuleException("Carrier does not have an active rate for this quote route/container.");

                var existingShipment = await _unitOfWork.Shipments.ExistsByQuoteIdAsync(request.QuoteId);
                if (existingShipment)
                    throw new BusinessRuleException("This quote is already used.");

                var shipment = new Domain.Entities.Shipments.Shipment
                {
                    QuoteId = quote.Id,
                    CustomerId = quote.CustomerId,
                    RouteId = quote.RouteId,
                    ContainerTypeId = quote.ContainerTypeId,
                    CarrierId = quote.CarrierId,
                    AgreedPrice = quote.FinalPrice,
                    Currency = quote.Currency,
                    Status = ShipmentStatus.Created,
                    CreatedAt = DateTimeOffset.UtcNow
                };

                var audit = new AuditLog
                {
                    CreatedAt = shipment.CreatedAt,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null,
                    NewValues = JsonSerializer.Serialize(shipment),
                    UserId = userId
                };

                await _unitOfWork.Shipments.AddAsync(shipment);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ShipmentResponse>(shipment);
            });
        }

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null || user.CustomerProfile == null)
                    return false;

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
                if (shipment == null || shipment.CustomerId != user.CustomerProfile.Id)
                    return false;

                if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                    throw new BusinessRuleException("Cannot delete delivered shipment.");

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(shipment),
                    NewValues = "Deleted",
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                _unitOfWork.Shipments.Delete(shipment);
                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }

        public async Task<ShipmentResponse?> UpdateAsync(Guid id, UpdateShipmentRequest request, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
                if (shipment == null)
                    return null;

                if (shipment.Status is ShipmentStatus.Delivered or ShipmentStatus.Closed or ShipmentStatus.Cancelled)
                    throw new BusinessRuleException("Cannot update shipment after it is delivered, closed, or cancelled.");

                if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                    throw new BusinessRuleException("Cannot update shipment.");

                if (request.EstimatedDeparture.HasValue && request.EstimatedArrival.HasValue &&
                    request.EstimatedArrival.Value < request.EstimatedDeparture.Value)
                {
                    throw new BusinessRuleException("Estimated arrival cannot be before estimated departure.");
                }

                if (request.ActualDeparture.HasValue && request.ActualArrival.HasValue &&
                    request.ActualArrival.Value < request.ActualDeparture.Value)
                {
                    throw new BusinessRuleException("Actual arrival cannot be before actual departure.");
                }

                if (request.ActualDeparture.HasValue && request.EstimatedDeparture.HasValue &&
                    request.ActualDeparture.Value < request.EstimatedDeparture.Value.AddDays(-30))
                {
                    throw new BusinessRuleException("Actual departure date is not valid.");
                }

                if (request.ActualArrival.HasValue && request.ActualDeparture.HasValue &&
                    request.ActualArrival.Value < request.ActualDeparture.Value)
                {
                    throw new BusinessRuleException("Actual arrival cannot be before actual departure.");
                }

                var oldShipment = shipment;

                if (!string.IsNullOrWhiteSpace(request.BookingNumber))
                    shipment.BookingNumber = request.BookingNumber.Trim();

                if (!string.IsNullOrWhiteSpace(request.VesselName))
                    shipment.VesselName = request.VesselName.Trim();

                if (!string.IsNullOrWhiteSpace(request.VoyageNumber))
                    shipment.VoyageNumber = request.VoyageNumber.Trim();

                if (!string.IsNullOrWhiteSpace(request.CurrentCheckpoint))
                    shipment.CurrentCheckpoint = request.CurrentCheckpoint.Trim();

                if (request.EstimatedDeparture.HasValue)
                    shipment.EstimatedDeparture = request.EstimatedDeparture.Value;

                if (request.EstimatedArrival.HasValue)
                    shipment.EstimatedArrival = request.EstimatedArrival.Value;

                if (request.ActualDeparture.HasValue)
                    shipment.ActualDeparture = request.ActualDeparture.Value;

                if (request.ActualArrival.HasValue)
                    shipment.ActualArrival = request.ActualArrival.Value;

                shipment.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldShipment),
                    NewValues = JsonSerializer.Serialize(shipment),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<ShipmentResponse>(shipment);
            });
        }

        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
