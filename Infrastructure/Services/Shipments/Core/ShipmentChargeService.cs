using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentChargeService : IShipmentChargeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ShipmentChargeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentChargeResponse> CreateAsync(CreateShipmentChargeRequest request)
        {
            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Closed)
                throw new BusinessRuleException("Cannot add charges to a delivered/closed shipment.");

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

            if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Closed)
                throw new BusinessRuleException("Cannot delete charges from a delivered/closed shipment.");

            _unitOfWork.ShipmentCharges.Delete(charge);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ShipmentChargeResponse?> GetByIdAsync(Guid id)
        {
            var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

            if (charge == null)
                return null;

            return _mapper.Map<ShipmentChargeResponse>(charge);
        }

        public async Task<IReadOnlyList<ShipmentChargeResponse>> GetByShipmentIdAsync(Guid shipmentId)
        {
            var charges = await _unitOfWork.ShipmentCharges.GetByShipmentIdAsync(shipmentId);

            if (!charges.Any())
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

            if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Closed)
                throw new BusinessRuleException("Cannot update charges in a delivered/closed shipment.");

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
