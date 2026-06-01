using Application.ApplicationRules.Shipments;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        private static bool HasLockedInvoiceForItemModification(Domain.Entities.Shipments.Shipment shipment)
        {
            return shipment.Invoices.Any(x =>
                !x.IsDeleted &&
                ((x.NetShipmentPrice > 0 && x.PaymentStatus == PaymentStatus.Pending) ||
                 (x.NetShipmentPrice <= 0 &&
                  x.PaymentStatus is PaymentStatus.Pending or PaymentStatus.PartiallyPaid or PaymentStatus.Paid)));
        }

        public async Task<ShipmentItemResponse> CreateAsync(CreateShipmentItemRequest request, string userId, bool isPrivileged)
        {
            if (isPrivileged)
                throw new BusinessRuleException("Privileged users cannot modify shipment cargo items.");

            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (user.CustomerProfile == null)
                throw new BusinessRuleException("Customer profile not found.");

            var shipment = await _unitOfWork.Shipments
                .GetTrackedByIdWithDetailsAsync(request.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (shipment.CustomerId != user.CustomerProfile.Id)
                throw new BusinessRuleException("You do not have permission to modify this shipment.");

            var hasLockedInvoice = HasLockedInvoiceForItemModification(shipment);
            
            if (hasLockedInvoice)
                throw new BusinessRuleException("Cannot modify shipment items after invoice confirmation.");

            if (!ShipmentStatusRules.CanModifyItems(shipment.Status))
                throw new BusinessRuleException("Cannot add items at the current shipment status.");

            if (request.NetWeight > request.GrossWeight)
                throw new BusinessRuleException("Net weight cannot be greater than gross weight.");

            if (request.IsHazardous && !shipment.IsHazardousAllowed)
                throw new BusinessRuleException("Hazardous cargo is not allowed for this shipment.");

            var itemChargeableWeight = ShipmentWeightCalculator
            .CalculateItemChargeableWeight(request.GrossWeight,request.VolumeCbm);
            EnsureShipmentItemWithinAllowedLimits(shipment, oldGrossWeight: 0, oldVolumeCbm: 0,
            newGrossWeight: request.GrossWeight, newVolumeCbm: request.VolumeCbm);

            var shipmentItem = new ShipmentItem
            {
                ShipmentId = shipment.Id,
                CreatedAt = DateTimeOffset.UtcNow,
                Description = request.Description.Trim(),
                MarksAndNumbers = request.MarksAndNumbers?.Trim(),
                Quantity = request.Quantity,
                ChargeableWeight = itemChargeableWeight,
                GrossWeight = request.GrossWeight,
                VolumeCbm = request.VolumeCbm,
                NetWeight = request.NetWeight,
                IsHazardous = request.IsHazardous,
                RequiredTemperatureCelsius = request.RequiredTemperatureCelsius
            };

            RecalculateShipmentTotals(shipment);

            shipment.Items.Add(shipmentItem);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId, bool isPrivileged)
        {
            if (isPrivileged)
                throw new BusinessRuleException("Privileged users cannot modify cargo items.");

            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new Exception("User not found");

            if (user.CustomerProfile == null)
                throw new Exception("User not found");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null || shipmentItem.Shipment.CustomerId != user.CustomerProfile.Id)
                return false;

            if (user.CustomerProfile == null)
                throw new Exception("User not found");

            if (user.CustomerProfile.Id != shipmentItem.Shipment.CustomerId)
                throw new Exception("no access for this shipment");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyItems(shipment.Status))
                throw new BusinessRuleException("Cannot delete items from a delivered/closed shipment.");

            var hasLockedInvoice = HasLockedInvoiceForItemModification(shipment);

            if (hasLockedInvoice)
                throw new BusinessRuleException("Cannot modify shipment items after invoice confirmation.");

            RecalculateShipmentTotals(shipment);

            _unitOfWork.ShipmentItems.Delete(shipmentItem);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<ShipmentItemResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null)
                return null;

            if (isPrivileged)
                return _mapper.Map<ShipmentItemResponse>(shipmentItem);

            if (user.CustomerProfile == null)
                throw new BusinessRuleException("Customer profile not found.");

            if (shipmentItem.Shipment.CustomerId != user.CustomerProfile.Id)
                return null;

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }

        public async Task<IReadOnlyList<ShipmentItemResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var shipment = await _unitOfWork.Shipments.GetByIdWithDetailsAsync(shipmentId);
            if (shipment == null)
                return new List<ShipmentItemResponse>();

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null)
                    throw new BusinessRuleException("Customer profile not found.");

                if (shipment.CustomerId != user.CustomerProfile.Id)
                    return new List<ShipmentItemResponse>();
            }

            return _mapper.Map<IReadOnlyList<ShipmentItemResponse>>(shipment.Items);
        }

        public async Task<ShipmentItemResponse?> UpdateAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentItemRequest request)
        {
            if (isPrivileged)
                throw new BusinessRuleException("Privileged users cannot modify cargo items.");

            var user = await _userManager.Users
                    .Include(x => x.CustomerProfile)
                    .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new BusinessRuleException("User not found.");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null)
                return null;

            var currentShipment = await _unitOfWork.Shipments
                .GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);

            if (currentShipment == null || currentShipment.CustomerId != user.CustomerProfile.Id)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyItems(currentShipment.Status))
                throw new BusinessRuleException("Cannot update shipment items in the current shipment status.");

            if (request.ShipmentId.HasValue && request.ShipmentId.Value != shipmentItem.ShipmentId)
                throw new BusinessRuleException("Moving cargo items between shipments is not supported.");

            var hasLockedInvoice = HasLockedInvoiceForItemModification(currentShipment);

            if (hasLockedInvoice)
                throw new BusinessRuleException("Cannot modify shipment items after invoice confirmation.");

            var newGrossWeight = request.GrossWeight ?? shipmentItem.GrossWeight;
            var newNetWeight = request.NetWeight ?? shipmentItem.NetWeight;
            var newVolumeCbm = request.VolumeCbm ?? shipmentItem.VolumeCbm;

            if (newNetWeight > newGrossWeight)
                throw new BusinessRuleException("Net weight cannot be greater than gross weight.");

            var newItemChargeableWeight = ShipmentWeightCalculator.CalculateItemChargeableWeight(newGrossWeight, newVolumeCbm);
            EnsureShipmentItemWithinAllowedLimits( currentShipment, oldGrossWeight: shipmentItem.GrossWeight, oldVolumeCbm: shipmentItem.VolumeCbm,
            newGrossWeight: newGrossWeight, newVolumeCbm: newVolumeCbm);



            shipmentItem.Description = string.IsNullOrWhiteSpace(request.Description)
                ? shipmentItem.Description
                : request.Description.Trim();

            shipmentItem.Quantity = request.Quantity ?? shipmentItem.Quantity;
            shipmentItem.GrossWeight = newGrossWeight;
            shipmentItem.NetWeight = newNetWeight;
            shipmentItem.VolumeCbm = newVolumeCbm;
            shipmentItem.ChargeableWeight = newItemChargeableWeight;

            shipmentItem.IsHazardous = request.IsHazardous ?? shipmentItem.IsHazardous;
            shipmentItem.RequiredTemperatureCelsius =
                request.RequiredTemperatureCelsius ?? shipmentItem.RequiredTemperatureCelsius;

            shipmentItem.MarksAndNumbers = string.IsNullOrWhiteSpace(request.MarksAndNumbers)
                ? shipmentItem.MarksAndNumbers
                : request.MarksAndNumbers.Trim();

            shipmentItem.UpdatedAt = DateTimeOffset.UtcNow;

            RecalculateShipmentTotals(currentShipment);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentItemResponse>(shipmentItem);
        }

        private static void EnsureShipmentItemWithinAllowedLimits(Domain.Entities.Shipments.Shipment shipment, decimal oldGrossWeight,
        decimal oldVolumeCbm, decimal newGrossWeight, decimal newVolumeCbm)
        {
            var totalGrossAfterChange =
                shipment.Items.Sum(x => x.GrossWeight)
                - oldGrossWeight
                + newGrossWeight;

            var totalVolumeAfterChange =
                shipment.Items.Sum(x => x.VolumeCbm)
                - oldVolumeCbm
                + newVolumeCbm;

            var totalChargeableAfterChange =
                ShipmentWeightCalculator.CalculateShipmentChargeableWeight(
                    totalGrossAfterChange,
                    totalVolumeAfterChange);

            if (totalGrossAfterChange > shipment.AllowedGrossWeightKg)
                throw new BusinessRuleException("Gross weight exceeds the approved shipment limit.");

            if (totalVolumeAfterChange > shipment.AllowedVolumeCbm)
                throw new BusinessRuleException("Volume exceeds the approved shipment limit.");

            if (totalChargeableAfterChange > shipment.AllowedChargeableWeightKg)
                throw new BusinessRuleException("Chargeable weight exceeds the approved shipment limit.");
        }

        private static void RecalculateShipmentTotals(Domain.Entities.Shipments.Shipment shipment)
        {
            shipment.TotalGrossWeightKg =
                shipment.Items.Sum(x => x.GrossWeight);

            shipment.TotalNetWeightKg =
                shipment.Items.Sum(x => x.NetWeight);

            shipment.TotalVolumeCbm =
                shipment.Items.Sum(x => x.VolumeCbm);

            shipment.TotalChargeableWeightKg =
                ShipmentWeightCalculator.CalculateShipmentChargeableWeight(
                    shipment.TotalGrossWeightKg,
                    shipment.TotalVolumeCbm);
        }
    }
}
