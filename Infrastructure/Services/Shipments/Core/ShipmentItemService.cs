using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentItemService : IShipmentItemService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ShipmentItemService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentItemResponse> CreateAsync(CreateShipmentItemRequest request)
        {
            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId);
            if(shipment == null)
                throw new KeyNotFoundException($"Shipment with not found.");

            if(shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Closed)
                throw new BusinessRuleException($"Cannot add items to a delivered/closed shipment.");

            var shipmentItem = new ShipmentItem
            {
                ShipmentId = shipment.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                Description = request.Description,
                Quantity = request.Quantity,
                Weight = request.Weight
            };
            shipment.Items.Add(shipmentItem);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null)
                return false;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (shipment.Status == ShipmentStatus.Delivered || shipment.Status == ShipmentStatus.Closed)
                throw new BusinessRuleException("Cannot delete items from a delivered/closed shipment.");

            _unitOfWork.ShipmentItems.Delete(shipmentItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ShipmentItemResponse?> GetByIdAsync(Guid id)
        {
            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if(shipmentItem == null)
                return null;

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }

        public async Task<IReadOnlyList<ShipmentItemResponse>> GetByShipmentIdAsync(Guid shipmentId)
        {
            var shipmentItems = await _unitOfWork.ShipmentItems.GetByShipmentIdAsync(shipmentId);
            if(!shipmentItems.Any())
                return new List<ShipmentItemResponse>();

            return _mapper.Map<IReadOnlyList<ShipmentItemResponse>>(shipmentItems);
        }

        public async Task<ShipmentItemResponse?> UpdateAsync(Guid id, UpdateShipmentItemRequest request)
        {
            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null)
                return null;

            var currentShipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);

            if (currentShipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (currentShipment.Status == ShipmentStatus.Delivered || currentShipment.Status == ShipmentStatus.Closed)
                throw new BusinessRuleException("Cannot update items in a delivered/closed shipment.");

            if (!request.ShipmentId.HasValue)
                request.ShipmentId = shipmentItem.ShipmentId;

            if(string.IsNullOrWhiteSpace(request.Description))
                request.Description = shipmentItem.Description;

            if(!request.Quantity.HasValue)
                request.Quantity = shipmentItem.Quantity;

            if(!request.Weight.HasValue)
                request.Weight = shipmentItem.Weight;

            if(request.ShipmentId != shipmentItem.ShipmentId)
            {
                var newShipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId.Value);
                if(newShipment == null)
                    throw new KeyNotFoundException($"Shipment with not found.");
                if(newShipment.Status == ShipmentStatus.Delivered || newShipment.Status == ShipmentStatus.Closed)
                    throw new BusinessRuleException($"Cannot move item to a delivered/closed shipment.");

                shipmentItem.ShipmentId = request.ShipmentId.Value;
            }

            shipmentItem.Description = request.Description;
            shipmentItem.Quantity = request.Quantity.Value;
            shipmentItem.Weight = request.Weight.Value;
            shipmentItem.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }
    }
}
