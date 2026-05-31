using Application.Interfaces.Repositories.Patterns;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Helper
{
    public static class InvoiceHelper
    {
        public static string NormalizeAndValidateCurrency(string currency)
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

        public static string GenerateInvoiceNumber(string nationalId)
        {
            var now = DateTimeOffset.UtcNow;

            var customerPart = nationalId[..3];

            var datePart = now.ToString("MMdd");

            var randomPart = Random.Shared.Next(1000, 9999);

            return $"INV-{customerPart}-{datePart}-{randomPart}";
        }

        public static void EnsureInvoiceCanBePaid(Invoice invoice)
        {
            if (invoice.PaymentStatus == PaymentStatus.Paid)
                throw new BusinessRuleException("Invoice is already paid.");

            if (invoice.PaymentStatus == PaymentStatus.Draft)
                throw new BusinessRuleException("Invoice is draft.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");
        }

        public static async Task<ApplicationUser> GetUserOrThrowAsync(string userId, UserManager<ApplicationUser> _userManager)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return user;
        }

        public static async Task<Invoice> GetInvoiceOrThrowAsync(Guid invoiceId, IUnitOfWork _unitOfWork)
        {
            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);

            if (invoice == null)
                throw new KeyNotFoundException("Invoice not found.");

            return invoice;
        }

        public static async Task<Domain.Entities.Shipments.Shipment> GetShipmentOrThrowAsync(Guid shipmentId, IUnitOfWork _unitOfWork)
        {
            var shipment = await _unitOfWork.Shipments
                .GetTrackedByIdWithDetailsAsync(shipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            return shipment;
        }

        public static void EnsureCustomerOwnsShipment(ApplicationUser user, Domain.Entities.Shipments.Shipment shipment)
        {
            if (user.CustomerProfile == null)
                throw new KeyNotFoundException("Customer profile not found.");

            if (shipment.CustomerId != user.CustomerProfile.Id)
                throw new UnauthorizedAccessException("You do not have access to this shipment.");
        }

        public static async Task<(Invoice Invoice, Domain.Entities.Shipments.Shipment Shipment)>
        GetInvoiceContextAsync(Guid invoiceId, IUnitOfWork unitOfWork)
        {
            var invoice = await GetInvoiceOrThrowAsync(invoiceId, unitOfWork);

            var shipment = await GetShipmentOrThrowAsync(invoice.ShipmentId, unitOfWork);

            return (invoice, shipment);
        }

        public static async Task<(ApplicationUser User, Invoice Invoice, Domain.Entities.Shipments.Shipment Shipment)>
GetInvoiceContextAsync(Guid invoiceId, string userId, bool isPrivileged, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork)
        {
            var user = await GetUserOrThrowAsync(userId, userManager);

            var invoice = await GetInvoiceOrThrowAsync(invoiceId, unitOfWork);

            var shipment = await GetShipmentOrThrowAsync(invoice.ShipmentId, unitOfWork);

            if (!isPrivileged)
                EnsureCustomerOwnsShipment(user, shipment);

            return (user, invoice, shipment);
        }

        public static void EnsureInvoiceCanBeConfirmed(Invoice invoice)
        {
            if (invoice.PaymentStatus != PaymentStatus.Draft)
                throw new BusinessRuleException("Only draft invoices can be confirmed.");

            if (invoice.PaymentStatus == PaymentStatus.Paid)
                throw new BusinessRuleException("Invoice is already paid.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");
        }

        public static void EnsureInvoiceCanBePartiallyPaid(Invoice invoice)
        {
            if (invoice.PaymentStatus == PaymentStatus.Paid)
                throw new BusinessRuleException("Invoice is already paid.");

            if (invoice.PaymentStatus == PaymentStatus.Draft)
                throw new BusinessRuleException("Invoice is draft.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");
        }

        public static void EnsurePartialPaymentAmountIsValid(Invoice invoice, decimal amount)
        {
            var paidAmount = invoice.Payments
                .Where(p => p.Status == PaymentTransactionStatus.Succeeded)
                .Sum(p => p.Amount);

            if (amount <= 0)
                throw new BusinessRuleException("Payment amount must be greater than zero.");

            if (amount >= invoice.TotalAmount)
                throw new BusinessRuleException("Use full payment, not partial payment.");

            if (paidAmount + amount > invoice.TotalAmount)
                throw new BusinessRuleException("Total paid amount cannot exceed invoice amount.");
        }

        public static void ApplyPaymentStatus(Invoice invoice)
        {
            var paidAmount = invoice.Payments
                .Where(p => p.Status == PaymentTransactionStatus.Succeeded)
                .Sum(p => p.Amount);

            if (paidAmount >= invoice.TotalAmount)
            {
                invoice.PaymentStatus = PaymentStatus.Paid;
                invoice.PaidAt = DateTimeOffset.UtcNow;
            }
            else if (paidAmount > 0)
            {
                invoice.PaymentStatus = PaymentStatus.PartiallyPaid;
            }

            invoice.UpdatedAt = DateTimeOffset.UtcNow;
        }

        public static void EnsureInvoiceCanBeRefunded(Invoice invoice)
        {
            if (invoice.PaymentStatus == PaymentStatus.Draft)
                throw new BusinessRuleException("Invoice is draft.");

            if (invoice.PaymentStatus == PaymentStatus.Refunded)
                throw new BusinessRuleException("Invoice is already refunded.");

            if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                throw new BusinessRuleException("Invoice is cancelled.");
        }
    }
}
