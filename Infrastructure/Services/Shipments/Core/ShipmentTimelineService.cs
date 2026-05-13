using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentTimelineService : IShipmentTimelineService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public ShipmentTimelineService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IReadOnlyCollection<ShipmentTimelineItemResponse>> GetShipmentTimelineAsync(Guid shipmentId, QueryParameters query,
        string userId, bool isPrivileged)
        {
            var user = await _userManager.Users
                    .Include(x => x.CustomerProfile)
                    .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new BusinessRuleException("User not found.");

            var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);

            if (shipment == null)
                throw new BusinessRuleException("Shipment not found.");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || user.CustomerProfile.Id != shipment.CustomerId)
                    throw new BusinessRuleException("You do not have permission to view this shipment timeline.");
            }

            var statusHistory = await _unitOfWork.StatusHistoryRepositories.GetByShipmentIdForTimelineAsync(shipmentId);
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(shipmentId);
            var charges = await _unitOfWork.ShipmentCharges.GetByShipmentIdAsync(shipmentId);
            var items = await _unitOfWork.ShipmentItems.GetByShipmentIdAsync(shipmentId);
            

            var timeline = new List<ShipmentTimelineItemResponse>();

            timeline.AddRange(statusHistory.Select(x => new ShipmentTimelineItemResponse
            {
                Type = "StatusChanged",
                Category = "Shipment",
                Title = $"Shipment status changed from {x.FromStatus} to {x.ToStatus}",
                Description = x.Reason,
                Amount = null,
                Currency = null,
                CreatedAt = x.ChangedAt,
                CreatedBy = x.ChangedBy
            }));

            timeline.AddRange(invoices.Select(x => new ShipmentTimelineItemResponse
            {
                Type = "InvoiceCreated",
                Category = "Invoice",
                Title = $"Invoice {x.InvoiceNumber} created",
                Description =
                    $"Payment Status: {x.PaymentStatus}, " +
                    $"Payer: {x.PayerType}, " +
                    $"Subtotal: {x.SubTotal} {x.Currency}, " +
                    $"Tax: {x.TaxAmount} {x.Currency}, " +
                    $"Total: {x.TotalAmount} {x.Currency}, " +
                    $"Due Date: {x.DueDate:yyyy-MM-dd}" +
                    (x.PaidAt.HasValue
                        ? $", Paid At: {x.PaidAt:yyyy-MM-dd HH:mm}"
                        : "") +
                    (x.CancelledAt.HasValue
                        ? $", Cancelled: {x.CancellationReason}"
                        : ""),
                Amount = x.TotalAmount,
                Currency = x.Currency,
                CreatedAt = x.IssuedAt,
                CreatedBy = null
            }));

            timeline.AddRange(charges.Select(x => new ShipmentTimelineItemResponse
            {
                Type = "ChargeAdded",
                Category = "Charge",
                Title = $"Charge added: {x.Description}",
                Description =
                    $"Type: {x.ChargeType}, " +
                    $"Payer: {x.PayerType}, " +
                    $"Base Amount: {x.Amount} {x.Currency}, " +
                    $"Tax: {x.TaxAmount} {x.Currency}, " +
                    $"Total: {x.TotalAmount} {x.Currency}" +
                    (x.InvoiceId.HasValue
                        ? ", Linked to invoice"
                        : ""),
                Amount = x.TotalAmount,
                Currency = x.Currency,
                CreatedAt = x.CreatedAt,
                CreatedBy = null
            }));

            timeline.AddRange(items.Select(x => new ShipmentTimelineItemResponse
            {
                Type = "ItemAdded",
                Category = "Item",
                Title = $"Shipment item added: {x.Description}",
                Description =
                    $"Quantity: {x.Quantity}, " +
                    $"Chargeable Weight: {x.ChargeableWeight} kg, " +
                    $"Gross Weight: {x.GrossWeight} kg, " +
                    $"Net Weight: {x.NetWeight} kg, " +
                    $"Volume: {x.VolumeCbm} CBM" +
                    (x.IsHazardous ? ", Hazardous cargo" : "") +
                    (x.RequiredTemperatureCelsius.HasValue
                        ? $", Required Temp: {x.RequiredTemperatureCelsius.Value}°C"
                        : "") +
                    (!string.IsNullOrWhiteSpace(x.MarksAndNumbers)
                        ? $", Marks: {x.MarksAndNumbers}"
                        : ""),
                Amount = null,
                Currency = null,
                CreatedAt = x.CreatedAt,
                CreatedBy = null
            }));

            return timeline
                .OrderByDescending(x => x.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();
        }
    }
}
