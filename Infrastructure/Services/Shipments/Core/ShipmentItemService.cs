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
    public class ShipmentItemService : IShipmentItemService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public ShipmentItemService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<ShipmentItemResponse> CreateAsync(CreateShipmentItemRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                throw new Exception("User not found");
            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId);
            if(shipment == null || shipment.CustomerId != user.CustomerProfile.Id)
                throw new KeyNotFoundException($"Shipment with not found.");

            if(!ShipmentStatusRules.CanModifyItems(shipment.Status))
                throw new BusinessRuleException($"Cannot add items.");

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

        public async Task<bool> DeleteAsync(Guid id, string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                throw new Exception("User not found");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null || shipmentItem.Shipment.CustomerId != user.CustomerProfile.Id)
                return false;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyItems(shipment.Status))
                throw new BusinessRuleException("Cannot delete items from a delivered/closed shipment.");

            _unitOfWork.ShipmentItems.Delete(shipmentItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ShipmentItemResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if(shipmentItem == null)
                return null;

            if(isPrivileged)
                return _mapper.Map<ShipmentItemResponse>(shipmentItem);

            if(user.CustomerProfile == null)
                throw new Exception("User not found");

            if (user.CustomerProfile.Id != shipmentItem.Shipment.CustomerId)
                return null;

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }

        public async Task<IReadOnlyList<ShipmentItemResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null )
                throw new Exception("User not found");

            var shipment = await _unitOfWork.Shipments.GetByIdWithDetailsAsync(shipmentId);

            if (shipment == null)
                return new List<ShipmentItemResponse>();

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || shipment.CustomerId != user.CustomerProfile.Id)
                    return new List<ShipmentItemResponse>();
            }

            var shipmentItems = await _unitOfWork.ShipmentItems.GetByShipmentIdAsync(shipmentId);
            return _mapper.Map<IReadOnlyList<ShipmentItemResponse>>(shipmentItems);
        }

        public async Task<ShipmentItemResponse?> UpdateAsync(Guid id, string userId, UpdateShipmentItemRequest request)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.CustomerProfile == null)
                throw new Exception("User not found");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null)
                return null;

            var currentShipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);

            if (currentShipment == null || currentShipment.CustomerId != user.CustomerProfile.Id)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyItems(currentShipment.Status))
                throw new BusinessRuleException("Cannot update items.");

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
                if(newShipment == null || newShipment.CustomerId != user.CustomerProfile.Id)
                    throw new KeyNotFoundException($"Shipment with not found.");

                if(!ShipmentStatusRules.CanModifyItems(newShipment.Status))
                    throw new BusinessRuleException($"Cannot move item to.");

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
