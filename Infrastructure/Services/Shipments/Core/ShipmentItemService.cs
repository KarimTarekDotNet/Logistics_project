using Application.ApplicationRules.Shipments;
using Application.Common;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentItemService : IShipmentItemService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentItemService> _logger;

        public ShipmentItemService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<ShipmentItemService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        private static bool HasLockedInvoiceForItemModification(Domain.Entities.Shipments.Shipment shipment)
            => shipment.Invoices.Any(x => !x.IsDeleted && x.NetShipmentPrice > 0 && x.PaymentStatus == PaymentStatus.Pending);

        public async Task<Result<ShipmentItemResponse>> CreateAsync(CreateShipmentItemRequest request, string userId, bool isPrivileged)
        {
            _logger.LogInformation("Creating shipment item for shipment {ShipmentId} by user {UserId}", request.ShipmentId, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                if (isPrivileged)
                    return Result<ShipmentItemResponse>.Forbidden("Privileged users cannot modify shipment cargo items.");

                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) return Result<ShipmentItemResponse>.NotFound("User not found.");
                if (user.CustomerProfile == null) return Result<ShipmentItemResponse>.Failure("Customer profile not found.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId);
                if (shipment == null) return Result<ShipmentItemResponse>.NotFound("Shipment not found.");
                if (shipment.CustomerId != user.CustomerProfile.Id) return Result<ShipmentItemResponse>.Forbidden("You do not have permission to modify this shipment.");
                if (HasLockedInvoiceForItemModification(shipment)) return Result<ShipmentItemResponse>.Failure("Cannot modify shipment items after invoice confirmation.");
                if (!ShipmentStatusRules.CanModifyItems(shipment.Status)) return Result<ShipmentItemResponse>.Failure("Cannot add items at the current shipment status.");
                if (request.NetWeight > request.GrossWeight) return Result<ShipmentItemResponse>.Failure("Net weight cannot be greater than gross weight.");
                if (request.IsHazardous && !shipment.IsHazardousAllowed) return Result<ShipmentItemResponse>.Failure("Hazardous cargo is not allowed for this shipment.");

                var itemChargeableWeight = ShipmentWeightCalculator.CalculateItemChargeableWeight(request.GrossWeight, request.VolumeCbm);

                var limitCheck = EnsureShipmentItemWithinAllowedLimits(shipment, 0, 0, request.GrossWeight, request.VolumeCbm);
                if (limitCheck != null) return Result<ShipmentItemResponse>.Failure(limitCheck);

                var shipmentItem = new ShipmentItem
                {
                    ShipmentId = shipment.Id, CreatedAt = DateTimeOffset.UtcNow,
                    Description = request.Description.Trim(), MarksAndNumbers = request.MarksAndNumbers?.Trim(),
                    Quantity = request.Quantity, ChargeableWeight = itemChargeableWeight,
                    GrossWeight = request.GrossWeight, VolumeCbm = request.VolumeCbm,
                    NetWeight = request.NetWeight, IsHazardous = request.IsHazardous,
                    RequiredTemperatureCelsius = request.RequiredTemperatureCelsius
                };

                RecalculateShipmentTotals(shipment);
                shipment.Items.Add(shipmentItem);

                var audit = new AuditLog
                {
                    CreatedAt = shipmentItem.CreatedAt, EntityId = shipmentItem.Id,
                    EntityName = nameof(ShipmentItem).ToUpper(), Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null, NewValues = JsonSerializer.Serialize(shipmentItem), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("ShipmentItem {Id} created for shipment {ShipmentId}", shipmentItem.Id, request.ShipmentId);
                return Result<ShipmentItemResponse>.Success(_mapper.Map<ShipmentItemResponse>(shipmentItem), 201);
            });
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, string userId, bool isPrivileged)
        {
            _logger.LogInformation("Deleting shipment item {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                if (isPrivileged) return Result<bool>.Forbidden("Privileged users cannot modify cargo items.");

                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) return Result<bool>.NotFound("User not found.");
                if (user.CustomerProfile == null) return Result<bool>.NotFound("User not found.");

                var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
                if (shipmentItem == null || shipmentItem.Shipment.CustomerId != user.CustomerProfile.Id)
                    return Result<bool>.NotFound("Shipment item not found.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);
                if (shipment == null) return Result<bool>.NotFound("Shipment not found.");
                if (!ShipmentStatusRules.CanModifyItems(shipment.Status)) return Result<bool>.Failure("Cannot delete items from a delivered/closed shipment.");
                if (HasLockedInvoiceForItemModification(shipment)) return Result<bool>.Failure("Cannot modify shipment items after invoice confirmation.");

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = shipmentItem.Id,
                    EntityName = nameof(ShipmentItem).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(shipmentItem), NewValues = "Deleted", UserId = userId
                };

                RecalculateShipmentTotals(shipment);
                await _unitOfWork.AuditLog.Add(audit);
                _unitOfWork.ShipmentItems.Delete(shipmentItem);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("ShipmentItem {Id} deleted", id);
                return Result<bool>.Success(true);
            });
        }

        public async Task<Result<ShipmentItemResponse>> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return Result<ShipmentItemResponse>.NotFound("User not found.");

            var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
            if (shipmentItem == null) return Result<ShipmentItemResponse>.NotFound("Shipment item not found.");

            if (isPrivileged) return Result<ShipmentItemResponse>.Success(_mapper.Map<ShipmentItemResponse>(shipmentItem));

            if (user.CustomerProfile == null) return Result<ShipmentItemResponse>.Failure("Customer profile not found.");
            if (shipmentItem.Shipment.CustomerId != user.CustomerProfile.Id) return Result<ShipmentItemResponse>.Forbidden("Access denied.");

            return Result<ShipmentItemResponse>.Success(_mapper.Map<ShipmentItemResponse>(shipmentItem));
        }

        public async Task<Result<IReadOnlyList<ShipmentItemResponse>>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null) return Result<IReadOnlyList<ShipmentItemResponse>>.NotFound("User not found.");

            var shipment = await _unitOfWork.Shipments.GetByIdWithDetailsAsync(shipmentId);
            if (shipment == null) return Result<IReadOnlyList<ShipmentItemResponse>>.Success(new List<ShipmentItemResponse>());

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null) return Result<IReadOnlyList<ShipmentItemResponse>>.Failure("Customer profile not found.");
                if (shipment.CustomerId != user.CustomerProfile.Id) return Result<IReadOnlyList<ShipmentItemResponse>>.Forbidden("Access denied.");
            }

            return Result<IReadOnlyList<ShipmentItemResponse>>.Success(_mapper.Map<IReadOnlyList<ShipmentItemResponse>>(shipment.Items));
        }

        public async Task<Result<ShipmentItemResponse>> UpdateAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentItemRequest request)
        {
            _logger.LogInformation("Updating shipment item {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                if (isPrivileged) return Result<ShipmentItemResponse>.Forbidden("Privileged users cannot modify cargo items.");

                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null || user.CustomerProfile == null) return Result<ShipmentItemResponse>.Failure("User not found.");

                var shipmentItem = await _unitOfWork.ShipmentItems.GetByIdAsync(id);
                if (shipmentItem == null) return Result<ShipmentItemResponse>.NotFound("Shipment item not found.");

                var currentShipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentItem.ShipmentId);
                if (currentShipment == null || currentShipment.CustomerId != user.CustomerProfile.Id)
                    return Result<ShipmentItemResponse>.NotFound("Shipment not found.");

                if (!ShipmentStatusRules.CanModifyItems(currentShipment.Status)) return Result<ShipmentItemResponse>.Failure("Cannot update shipment items in the current shipment status.");
                if (request.ShipmentId.HasValue && request.ShipmentId.Value != shipmentItem.ShipmentId) return Result<ShipmentItemResponse>.Failure("Moving cargo items between shipments is not supported.");
                if (HasLockedInvoiceForItemModification(currentShipment)) return Result<ShipmentItemResponse>.Failure("Cannot modify shipment items after invoice confirmation.");

                var newGrossWeight = request.GrossWeight ?? shipmentItem.GrossWeight;
                var newNetWeight = request.NetWeight ?? shipmentItem.NetWeight;
                var newVolumeCbm = request.VolumeCbm ?? shipmentItem.VolumeCbm;

                if (newNetWeight > newGrossWeight) return Result<ShipmentItemResponse>.Failure("Net weight cannot be greater than gross weight.");

                var newItemChargeableWeight = ShipmentWeightCalculator.CalculateItemChargeableWeight(newGrossWeight, newVolumeCbm);
                var limitCheck = EnsureShipmentItemWithinAllowedLimits(currentShipment, shipmentItem.GrossWeight, shipmentItem.VolumeCbm, newGrossWeight, newVolumeCbm);
                if (limitCheck != null) return Result<ShipmentItemResponse>.Failure(limitCheck);

                var oldShipmentItem = shipmentItem;
                shipmentItem.Description = string.IsNullOrWhiteSpace(request.Description) ? shipmentItem.Description : request.Description.Trim();
                shipmentItem.Quantity = request.Quantity ?? shipmentItem.Quantity;
                shipmentItem.GrossWeight = newGrossWeight;
                shipmentItem.NetWeight = newNetWeight;
                shipmentItem.VolumeCbm = newVolumeCbm;
                shipmentItem.ChargeableWeight = newItemChargeableWeight;
                shipmentItem.IsHazardous = request.IsHazardous ?? shipmentItem.IsHazardous;
                shipmentItem.RequiredTemperatureCelsius = request.RequiredTemperatureCelsius ?? shipmentItem.RequiredTemperatureCelsius;
                shipmentItem.MarksAndNumbers = string.IsNullOrWhiteSpace(request.MarksAndNumbers) ? shipmentItem.MarksAndNumbers : request.MarksAndNumbers.Trim();
                shipmentItem.UpdatedAt = DateTimeOffset.UtcNow;

                RecalculateShipmentTotals(currentShipment);

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = shipmentItem.Id,
                    EntityName = nameof(ShipmentItem).ToUpper(), Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldShipmentItem), NewValues = JsonSerializer.Serialize(shipmentItem), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("ShipmentItem {Id} updated", id);
                return Result<ShipmentItemResponse>.Success(_mapper.Map<ShipmentItemResponse>(shipmentItem));
            });
        }

        private static string? EnsureShipmentItemWithinAllowedLimits(Domain.Entities.Shipments.Shipment shipment, decimal oldGrossWeight, decimal oldVolumeCbm, decimal newGrossWeight, decimal newVolumeCbm)
        {
            var totalGrossAfterChange = shipment.Items.Sum(x => x.GrossWeight) - oldGrossWeight + newGrossWeight;
            var totalVolumeAfterChange = shipment.Items.Sum(x => x.VolumeCbm) - oldVolumeCbm + newVolumeCbm;
            var totalChargeableAfterChange = ShipmentWeightCalculator.CalculateShipmentChargeableWeight(totalGrossAfterChange, totalVolumeAfterChange);

            if (totalGrossAfterChange > shipment.AllowedGrossWeightKg) return "Gross weight exceeds the approved shipment limit.";
            if (totalVolumeAfterChange > shipment.AllowedVolumeCbm) return "Volume exceeds the approved shipment limit.";
            if (totalChargeableAfterChange > shipment.AllowedChargeableWeightKg) return "Chargeable weight exceeds the approved shipment limit.";
            return null;
        }

        private static void RecalculateShipmentTotals(Domain.Entities.Shipments.Shipment shipment)
        {
            shipment.TotalGrossWeightKg = shipment.Items.Sum(x => x.GrossWeight);
            shipment.TotalNetWeightKg = shipment.Items.Sum(x => x.NetWeight);
            shipment.TotalVolumeCbm = shipment.Items.Sum(x => x.VolumeCbm);
            shipment.TotalChargeableWeightKg = ShipmentWeightCalculator.CalculateShipmentChargeableWeight(shipment.TotalGrossWeightKg, shipment.TotalVolumeCbm);
        }

        private async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(ShipmentItemService));
                throw;
            }
        }
    }
}
