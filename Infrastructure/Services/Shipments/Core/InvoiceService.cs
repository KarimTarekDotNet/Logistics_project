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

        public async Task<InvoiceResponse> CreateOrUpdateDraftInvoiceAsync(Guid shipmentId, string userId)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new KeyNotFoundException("user not found.");

            var shipment = await _unitOfWork.Shipments
                .GetTrackedByIdWithDetailsAsync(shipmentId);

            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found.");

            if (shipment.CustomerId != user.CustomerProfile.Id)
                throw new UnauthorizedAccessException("You do not have access to this invoice.");

            if (!ShipmentStatusRules.CanCreateInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot create invoice for the current shipment status.");

            if (shipment.Customer == null)
                throw new BusinessRuleException("Shipment customer data is required.");

            if (string.IsNullOrWhiteSpace(shipment.Customer.NationalId))
                throw new BusinessRuleException("Customer national id is required.");

            var charges = shipment.Charges
                .Where(x => !x.IsDeleted)
                .ToList();

            if (!charges.Any())
                throw new BusinessRuleException("No charges found to create invoice.");

            var draftInvoice = shipment.Invoices
                .FirstOrDefault(x =>
                    x.PaymentStatus == PaymentStatus.Draft &&
                    !x.IsDeleted);

            if (draftInvoice == null)
            {
                draftInvoice = new Invoice
                {
                    ShipmentId = shipment.Id,
                    InvoiceNumber = InvoiceHelper.GenerateInvoiceNumber(shipment.Customer.NationalId),
                    Currency = InvoiceHelper.NormalizeAndValidateCurrency(shipment.Currency),
                    PaymentStatus = PaymentStatus.Draft,
                    IssuedAt = DateTimeOffset.UtcNow,
                    DueDate = DateTimeOffset.UtcNow.AddDays(7),
                    CreatedAt = DateTimeOffset.UtcNow,
                    PayerType = charges.First().PayerType
                };

                await _unitOfWork.Invoices.AddAsync(draftInvoice);
            }
            else
            {
                draftInvoice.UpdatedAt = DateTimeOffset.UtcNow;
                draftInvoice.Charges.Clear();
            }

            foreach (var charge in charges)
            {
                draftInvoice.Charges.Add(charge);
            }

            draftInvoice.SubTotal = charges.Sum(x => x.Amount);
            draftInvoice.TaxAmount = charges.Sum(x => x.TaxAmount);
            draftInvoice.TotalAmount = draftInvoice.SubTotal + draftInvoice.TaxAmount;
            draftInvoice.Currency = shipment.Currency;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(draftInvoice);
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
            var context = await InvoiceHelper
            .GetInvoiceContextAsync(id, userId, isPrivileged, _userManager, _unitOfWork);

            return _mapper.Map<InvoiceResponse>(context.Invoice);
        }

        public async Task<IReadOnlyList<InvoiceResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await InvoiceHelper.GetUserOrThrowAsync(userId, _userManager);

            var shipment = await InvoiceHelper.GetShipmentOrThrowAsync(shipmentId, _unitOfWork);

            if (!isPrivileged)
                InvoiceHelper.EnsureCustomerOwnsShipment(user, shipment);

            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(shipmentId);

            if (!invoices.Any())
                throw new KeyNotFoundException("Invoice not found.");

            return _mapper.Map<IReadOnlyList<InvoiceResponse>>(invoices);
        }

        public async Task<InvoiceResponse?> ConfirmAsync(Guid id, string userId)
        {
            var (_, invoice, shipment) = await InvoiceHelper
            .GetInvoiceContextAsync(id, userId, isPrivileged: false, _userManager, _unitOfWork);

            if (!ShipmentStatusRules.CanPayInvoice(shipment.Status))
                throw new BusinessRuleException("Cannot confirm invoice for the current shipment status.");

            InvoiceHelper.EnsureInvoiceCanBeConfirmed(invoice);

            invoice.UpdatedAt = DateTimeOffset.UtcNow;
            invoice.PaymentStatus = PaymentStatus.Pending;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<InvoiceResponse>(invoice);
        }
    }
}
