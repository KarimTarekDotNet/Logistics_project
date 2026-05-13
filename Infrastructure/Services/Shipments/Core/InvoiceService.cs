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
    public class InvoiceService : IInvoiceService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request)
        {
            var shipment = await _unitOfWork.Shipments
                .GetTrackedByIdWithDetailsAsync(request.ShipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanCreateInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot create invoice for the current shipment status.");

            if (shipment.Customer == null)
                throw new BusinessRuleException("Shipment customer data is required to create invoice.");

            if (string.IsNullOrWhiteSpace(shipment.Customer.NationalId))
                throw new BusinessRuleException("Customer national id is required to generate invoice number.");

            var shipmentCharges = new List<ShipmentCharge>();

            foreach (var id in request.ShipmentChargeIds)
            {
                var charge = await _unitOfWork.ShipmentCharges.GetByIdAsync(id);

                if (charge == null)
                    throw new KeyNotFoundException($"Shipment charge '{id}' not found.");

                if (charge.ShipmentId != shipment.Id)
                    throw new BusinessRuleException("All charges must belong to the selected shipment.");

                shipmentCharges.Add(charge);
            }

            var currency = NormalizeAndValidateCurrency(request.Currency);

            var invoice = _mapper.Map<Invoice>(request);

            invoice.Currency = currency;
            invoice.Charges = shipmentCharges;
            invoice.SubTotal = shipmentCharges.Sum(x => x.Amount);
            invoice.TaxAmount = shipmentCharges.Sum(x => x.TaxAmount);
            invoice.TotalAmount = invoice.SubTotal + invoice.TaxAmount;
            invoice.InvoiceNumber = GenerateInvoiceNumber(shipment.Customer.NationalId);
            invoice.IssuedAt = DateTimeOffset.UtcNow;
            invoice.DueDate = request.DueDate;

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(invoice);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if(invoice == null)
                return false;

            if (invoice.PaymentStatus == PaymentStatus.PartiallyPaid || invoice.PaymentStatus == PaymentStatus.Paid)
                throw new BusinessRuleException("Paid or partially paid invoices cannot be cancelled.");

            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            _unitOfWork.Invoices.Delete(invoice);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<InvoiceResponse?> CancelAsync(Guid id, string userId, bool isPrivileged, string reason)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            
            if (user == null) 
                throw new KeyNotFoundException("User not found.");

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(invoice.ShipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanCancelInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot cancel invoice for the current shipment status.");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null)
                    throw new KeyNotFoundException("Customer profile not found.");

                if (shipment.CustomerId != user.CustomerProfile.Id)
                    throw new UnauthorizedAccessException("You do not have access to this invoice.");

                if (invoice.PaymentStatus != PaymentStatus.Pending)
                    throw new BusinessRuleException("Only pending invoices can be cancelled by customer.");
            }
            else
            {
                if (invoice.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyPaid)
                    throw new BusinessRuleException("Paid or partially paid invoices cannot be cancelled.");
            }

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is already cancelled.");

            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleException("Cancellation reason is required.");

            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            invoice.PaymentStatus = PaymentStatus.Cancelled;

            invoice.CancelledAt = DateTimeOffset.UtcNow;
            invoice.CancelledByUserId = userId;
            invoice.CancellationReason = reason.Trim();

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(invoice);
        }
        public async Task<InvoiceResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(invoice.ShipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null)
                    throw new KeyNotFoundException("Customer profile not found.");

                if (shipment.CustomerId != user.CustomerProfile.Id)
                    throw new UnauthorizedAccessException("You do not have access to this shipment.");
            }

            return _mapper.Map<InvoiceResponse>(invoice);
        }

        public async Task<IReadOnlyList<InvoiceResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(shipmentId);
            if (!invoices.Any())
                throw new KeyNotFoundException("Invoice not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null)
                    throw new KeyNotFoundException("Customer profile not found.");

                if (shipment.CustomerId != user.CustomerProfile.Id)
                    throw new UnauthorizedAccessException("You do not have access to this shipment.");
            }

            return _mapper.Map<IReadOnlyList<InvoiceResponse>>(invoices);
        }

        public async Task<InvoiceResponse?> MarkAsPaidAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(invoice.ShipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if(!ShipmentStatusRules.CanPayInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot pay invoice for the current shipment status.");

            if (invoice.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyPaid)
                throw new BusinessRuleException("Invoice is already paid/partially paid.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");

            invoice.PaidAt = DateTimeOffset.UtcNow;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            invoice.PaymentStatus = PaymentStatus.Paid;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InvoiceResponse>(invoice);
        }
        public async Task<InvoiceResponse?> MarkAsPartiallyPaidAsync(Guid id)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(invoice.ShipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanPartiallyPayInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot partially pay invoice for the current shipment status.");

            if (invoice.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyPaid)
                throw new BusinessRuleException("Invoice is already paid/partially paid.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");

            invoice.PaidAt = DateTimeOffset.UtcNow;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InvoiceResponse>(invoice);
        }
        public async Task<InvoiceResponse?> MarkAsRefundedAsync(Guid id)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(invoice.ShipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (!ShipmentStatusRules.CanRefundInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot refund invoice for the current shipment status.");

            if (invoice.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyPaid)
                throw new BusinessRuleException("Invoice is paid/partially paid.");

            if (invoice.PaymentStatus is PaymentStatus.Refunded)
                throw new BusinessRuleException("Invoice is already refunded.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");

            invoice.PaidAt = DateTimeOffset.UtcNow;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            invoice.PaymentStatus = PaymentStatus.Refunded;
            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<InvoiceResponse>(invoice);
        }

        private static string NormalizeAndValidateCurrency(string currency)
        {
            if (string.IsNullOrWhiteSpace(currency))
                throw new BusinessRuleException("Currency is required.");

            var normalizedCurrency = currency.Trim().ToUpperInvariant();

            var allowedCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "USD",
                "EGP",
                "EUR"
            };

            if (!allowedCurrencies.Contains(normalizedCurrency))
                throw new BusinessRuleException("Unsupported currency.");

            return normalizedCurrency;
        }

        private string GenerateInvoiceNumber(string nationalId)
        {
            var now = DateTimeOffset.UtcNow;

            var customerPart = nationalId[..3];

            var datePart = now.ToString("MMdd");

            var randomPart = Random.Shared.Next(1000, 9999);

            return $"INV-{customerPart}-{datePart}-{randomPart}";
        }
    }
}
