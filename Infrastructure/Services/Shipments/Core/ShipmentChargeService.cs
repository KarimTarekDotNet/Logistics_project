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
using Microsoft.EntityFrameworkCore;

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

        private static bool IsAttachedToCancelledInvoice(Domain.Entities.Shipments.Shipment shipment, ShipmentCharge charge)
        {
            return charge.InvoiceId.HasValue &&
                shipment.Invoices.Any(x =>
                    x.Id == charge.InvoiceId.Value &&
                    !x.IsDeleted &&
                    x.PaymentStatus == PaymentStatus.Cancelled);
        }

        public async Task<IEnumerable<ShipmentChargeResponse>> GenerateAsync(GenerateShipmentChargesRequest request, string userId)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null || user.CustomerProfile == null)
                throw new KeyNotFoundException("User not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(request.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (shipment.CustomerId != user.CustomerProfile.Id)
                throw new BusinessRuleException("You do not have permission to modify this shipment.");

            if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                throw new BusinessRuleException("Cannot modify charges at this stage.");

            var rules = await _unitOfWork.ShipmentChargeRule.GetActiveRulesAsync(shipment.Currency);

            if (!rules.Any())
                throw new BusinessRuleException("No active charge rules found.");

            var generatedCharges = new List<ShipmentCharge>();
            var hasBaseFreightInvoice = shipment.Invoices.Any(x => !x.IsDeleted && x.NetShipmentPrice > 0);


            foreach (var rule in rules)
            {
                if (rule.ChargeType == ChargeType.OceanFreight && hasBaseFreightInvoice)
                    continue;

                var unchargedItems = shipment.Items
                    .Where(item => !item.IsDeleted &&
                        !item.ChargeItems.Any(ci =>
                            ci.ShipmentCharge.ChargeType == rule.ChargeType &&
                            !ci.ShipmentCharge.IsDeleted &&
                            !IsAttachedToCancelledInvoice(shipment, ci.ShipmentCharge)))
                    .ToList();

                if (!unchargedItems.Any())
                    continue;

                var totalChargeableWeight = unchargedItems.Sum(x => x.ChargeableWeight);
                var totalVolume = unchargedItems.Sum(x => x.VolumeCbm);

                var amount = rule.CalculationType switch
                {
                    ChargeCalculationType.Fixed =>
                        rule.Value,

                    ChargeCalculationType.PerKg =>
                        totalChargeableWeight * rule.Value,

                    ChargeCalculationType.PerCbm =>
                        totalVolume * rule.Value,

                    ChargeCalculationType.PercentageOfAgreedPrice =>
                        shipment.AgreedPrice * rule.Value / 100m,

                    _ => throw new BusinessRuleException("Unsupported charge calculation type.")
                };

                var charge = new ShipmentCharge
                {
                    ShipmentId = shipment.Id,
                    ChargeType = rule.ChargeType,
                    PayerType = request.PayerType,
                    Amount = amount,
                    TaxAmount = 0.14m * amount,
                    Currency = shipment.Currency,
                    Description = $"{rule.ChargeType} auto generated charge",
                    CreatedAt = DateTimeOffset.UtcNow,
                    ChargeItems = unchargedItems.Select(item => new ShipmentChargeItem
                    {
                        ShipmentItemId = item.Id
                    }).ToList()
                };

                generatedCharges.Add(charge);
            }

            if (!generatedCharges.Any())
                return Enumerable.Empty<ShipmentChargeResponse>();

            await _unitOfWork.ShipmentCharges.AddRangeAsync(generatedCharges);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<IEnumerable<ShipmentChargeResponse>>(generatedCharges);
        }

        public async Task<bool> DeleteAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!isPrivileged && user.CustomerProfile == null)
                throw new BusinessRuleException("Customer profile not found.");

            var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

            if (charge == null)
                return false;

            if (!isPrivileged && charge.Shipment.CustomerId != user.CustomerProfile!.Id)
                return false;

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(charge.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanModifyCharges(shipment.Status))
                throw new BusinessRuleException("Cannot modify charges at this stage.");

            var invoice = charge.InvoiceId.HasValue
                ? shipment.Invoices.FirstOrDefault(x => x.Id == charge.InvoiceId.Value && !x.IsDeleted)
                : null;

            if (!isPrivileged)
            {
                if (charge.ChargeType == ChargeType.OceanFreight || invoice?.NetShipmentPrice > 0)
                    throw new BusinessRuleException("Base freight charges cannot be deleted by customer.");

                if (invoice?.PaymentStatus is PaymentStatus.PartiallyPaid or PaymentStatus.Paid)
                    throw new BusinessRuleException("Cannot delete charges from a confirmed invoice.");
            }
            else if (invoice?.PaymentStatus is PaymentStatus.PartiallyPaid or PaymentStatus.Paid)
            {
                throw new BusinessRuleException("Paid or partially paid invoice charges cannot be deleted.");
            }

            _unitOfWork.ShipmentCharges.Delete(charge);

            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<ShipmentChargeResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            if (!isPrivileged && user.CustomerProfile == null)
                throw new KeyNotFoundException("Customer profile not found.");

            var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

            if (charge == null)
                return null;

            if (!isPrivileged && charge.Shipment.CustomerId != user.CustomerProfile!.Id)
                return null;

            return _mapper.Map<ShipmentChargeResponse>(charge);
        }

        public async Task<IReadOnlyList<ShipmentChargeResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users
                    .Include(x => x.CustomerProfile)
                    .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null)
                    throw new BusinessRuleException("Customer profile not found.");

                if (shipment.CustomerId != user.CustomerProfile.Id)
                    throw new BusinessRuleException("You do not have permission.");
            }

            var charges = await _unitOfWork.ShipmentCharges.GetByShipmentIdAsync(shipmentId);

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

            if (!string.IsNullOrWhiteSpace(request.Currency))
                charge.Currency = request.Currency;

            if (request.ChargeType.HasValue)
                charge.ChargeType = request.ChargeType.Value;

            if (request.PayerType.HasValue)
                charge.PayerType = request.PayerType.Value;

            charge.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentChargeResponse>(charge);
        }
    }
}
