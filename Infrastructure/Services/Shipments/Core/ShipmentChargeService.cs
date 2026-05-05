using Application.ApplicationRules.Shipments;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentChargeService : IShipmentChargeService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ShipmentChargeService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ShipmentChargeResponse> CreateAsync(CreateShipmentChargeRequest request)
        {
            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                throw new BusinessRuleException("Cannot modify charges at this stage.");

            var charge = new ShipmentCharge
            {
                ShipmentId = shipment.Id,
                Description = request.Description,
                Amount = request.Amount,
                CreatedAt = DateTimeOffset.UtcNow
            };

            shipment.Charges.Add(charge);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentChargeResponse>(charge);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

            if (charge == null)
                return false;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(charge.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                throw new BusinessRuleException("Cannot modify charges at this stage.");

            _unitOfWork.ShipmentCharges.Delete(charge);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ShipmentChargeResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                throw new KeyNotFoundException("User not found.");

            var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

            if (charge == null || (charge.Shipment.CustomerId != user.CustomerProfile.Id && !isPrivileged))
                return null;

            return _mapper.Map<ShipmentChargeResponse>(charge);
        }

        public async Task<IReadOnlyList<ShipmentChargeResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                throw new KeyNotFoundException("User not found.");

            var charges = await _unitOfWork.ShipmentCharges.GetByShipmentIdAsync(shipmentId);

            if (!charges.Any(x => x.Shipment.CustomerId == user.CustomerProfile.Id || isPrivileged))
                return new List<ShipmentChargeResponse>();

            return _mapper.Map<IReadOnlyList<ShipmentChargeResponse>>(charges);
        }

        public async Task<ShipmentChargeResponse?> UpdateAsync(Guid id, UpdateShipmentChargeRequest request)
        {
            var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

            if (charge == null)
                return null;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(charge.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                throw new BusinessRuleException("Cannot modify charges at this stage.");

            if (!string.IsNullOrWhiteSpace(request.Description))
                charge.Description = request.Description;

            if (request.Amount.HasValue)
                charge.Amount = request.Amount.Value;

            charge.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentChargeResponse>(charge);
        }
    }
}
