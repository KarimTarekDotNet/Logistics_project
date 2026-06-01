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
    public class InvoicePaymentService : IInvoicePaymentService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public InvoicePaymentService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<InvoicePaymentResponse>> GetPaymentsByInvoiceIdAsync(Guid invoiceId, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var invoice = await _unitOfWork.Invoices.GetByIdAsync(invoiceId);
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
                    throw new UnauthorizedAccessException("You do not have access to this invoice.");
            }

            var payments =  isPrivileged ? await _unitOfWork.InvoicePayments.GetByInvoiceIdAsync(invoiceId)
            : await _unitOfWork.InvoicePayments.GetByInvoiceIdAsync(invoiceId, user.CustomerProfile!.Id);

            return _mapper.Map<IReadOnlyList<InvoicePaymentResponse>>(payments);
        }

        public async Task<InvoiceResponse?> MarkAsPaidAsync(Guid invoiceId, CreateInvoicePaymentRequest request)
        {
            var ( invoice, shipment) = await InvoiceHelper.GetInvoiceContextAsync(invoiceId, _unitOfWork);

            if (!ShipmentStatusRules.CanPayInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot pay invoice for the current shipment status.");

            InvoiceHelper.EnsureInvoiceCanBePaid(invoice);

            var previousPayments = await _unitOfWork.InvoicePayments.GetByInvoiceIdAsync(invoiceId);

            var alreadyPaid = previousPayments
                .Where(x => x.Status == PaymentTransactionStatus.Succeeded)
                .Sum(x => x.Amount);

            var remainingAmount = invoice.TotalAmount - alreadyPaid;

            if (request.Amount <= 0)
                throw new BusinessRuleException("Payment amount must be greater than zero.");

            if (request.Amount > remainingAmount)
                throw new BusinessRuleException("Payment amount exceeds remaining invoice amount.");

            if (request.Amount != remainingAmount)
                throw new BusinessRuleException("Payment amount must equal remaining amount to mark invoice as paid.");

            var payment = new InvoicePayment
            {
                InvoiceId = invoice.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = PaymentMethod.Cash,
                PaymentProvider = PaymentProvider.Manual,
                Status = PaymentTransactionStatus.Succeeded,
                TransactionId = Guid.NewGuid().ToString(),
                ReferenceNumber = null,
                PaidAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await _unitOfWork.InvoicePayments.AddAsync(payment);
            invoice.PaymentStatus = PaymentStatus.Paid;
            invoice.PaidAt = payment.PaidAt;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(invoice);
        }

        public async Task<InvoiceResponse?> MarkAsPartiallyPaidAsync(Guid invoiceId, CreateInvoicePaymentRequest request)
        {
            var ( invoice, shipment) = await InvoiceHelper.GetInvoiceContextAsync(invoiceId, _unitOfWork);

            if (!ShipmentStatusRules.CanPartiallyPayInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot pay invoice for the current shipment status.");

            InvoiceHelper.EnsureInvoiceCanBePartiallyPaid(invoice);

            InvoiceHelper.EnsurePartialPaymentAmountIsValid(invoice, request.Amount);

            var previousPayments = await _unitOfWork.InvoicePayments.GetByInvoiceIdAsync(invoiceId);

            var alreadyPaid = previousPayments
            .Where(x => x.Status == PaymentTransactionStatus.Succeeded)
            .Sum(x => x.Amount);

            var remainingAmount = invoice.TotalAmount - alreadyPaid;

            if (request.Amount <= 0)
                throw new BusinessRuleException("Payment amount must be greater than zero.");

            if (request.Amount > remainingAmount)
                throw new BusinessRuleException("Payment amount exceeds remaining invoice amount.");

            var payment = new InvoicePayment
            {
                InvoiceId = invoice.Id,
                Amount = request.Amount,
                Currency = request.Currency,
                PaymentMethod = PaymentMethod.Cash,
                PaymentProvider = PaymentProvider.Manual,
                Status = PaymentTransactionStatus.Succeeded,
                TransactionId = Guid.NewGuid().ToString(),
                ReferenceNumber = request.ReferenceNumber,
                PaidAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            };

            invoice.Payments.Add(payment);

            InvoiceHelper.ApplyPaymentStatus(invoice);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(invoice);
        }

        public async Task<InvoiceResponse?> MarkAsRefundedAsync(Guid invoiceId)
        {
            var invoice = await InvoiceHelper.GetInvoiceOrThrowAsync(invoiceId, _unitOfWork);

            var shipment = await InvoiceHelper.GetShipmentOrThrowAsync(invoice.ShipmentId, _unitOfWork);

            if (!ShipmentStatusRules.CanRefundInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot refund invoice for the current shipment status.");

            InvoiceHelper.EnsureInvoiceCanBeRefunded(invoice);

            var payments = await _unitOfWork.InvoicePayments.GetByInvoiceIdAsync(invoiceId);

            var successfulPayments = payments.Where(x => x.Status == PaymentTransactionStatus.Succeeded).ToList();

            if (!successfulPayments.Any())
                throw new BusinessRuleException("No successful payments found.");

            foreach (var payment in successfulPayments)
            {
                payment.Status = PaymentTransactionStatus.Refunded;
            }

            invoice.PaymentStatus = PaymentStatus.Refunded;
            invoice.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(invoice);
        }
    }
}
