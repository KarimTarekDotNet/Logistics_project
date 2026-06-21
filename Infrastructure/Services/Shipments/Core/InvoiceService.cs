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
    public class InvoiceService : IInvoiceService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<InvoiceService> _logger;

        public InvoiceService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<InvoiceService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        private static bool IsBaseFreightCharge(Domain.Entities.Shipments.Shipment shipment, ShipmentCharge charge)
        {
            if (charge.ChargeType != ChargeType.OceanFreight) return false;
            var invoice = charge.InvoiceId.HasValue
                ? shipment.Invoices.FirstOrDefault(x => x.Id == charge.InvoiceId.Value && !x.IsDeleted)
                : null;
            return invoice?.NetShipmentPrice > 0;
        }

        public async Task<Result<InvoiceResponse>> CreateOrUpdateDraftInvoiceAsync(Guid shipmentId, string userId)
        {
            _logger.LogInformation("Creating/updating draft invoice for shipment {ShipmentId} by user {UserId}", shipmentId, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null || user.CustomerProfile == null)
                    return Result<InvoiceResponse>.NotFound("User not found.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(shipmentId);
                if (shipment == null) return Result<InvoiceResponse>.NotFound("Shipment not found.");
                if (shipment.CustomerId != user.CustomerProfile.Id) return Result<InvoiceResponse>.Unauthorized("You do not have access to this invoice.");
                if (!ShipmentStatusRules.CanCreateInvoice(shipment.Status)) return Result<InvoiceResponse>.Failure("Cannot create invoice for the current shipment status.");
                if (shipment.Customer == null) return Result<InvoiceResponse>.Failure("Shipment customer data is required.");
                if (string.IsNullOrWhiteSpace(shipment.Customer.NationalId)) return Result<InvoiceResponse>.Failure("Customer national id is required.");

                var draftInvoice = shipment.Invoices.FirstOrDefault(x => x.PaymentStatus == PaymentStatus.Draft && !x.IsDeleted);

                var charges = shipment.Charges
                    .Where(x => !x.IsDeleted)
                    .Where(x => x.InvoiceId == null || x.InvoiceId == draftInvoice?.Id)
                    .Where(x => !IsBaseFreightCharge(shipment, x))
                    .ToList();

                if (!charges.Any()) return Result<InvoiceResponse>.Failure("No charges found to create invoice.");
                if (charges.Select(x => x.PayerType).Distinct().Count() > 1) return Result<InvoiceResponse>.Failure("Cannot create one invoice for multiple payer types.");

                var isCreate = draftInvoice == null;
                if (isCreate)
                {
                    draftInvoice = new Invoice
                    {
                        ShipmentId = shipment.Id,
                        InvoiceNumber = InvoiceHelper.GenerateInvoiceNumber(shipment.Customer.NationalId),
                        Currency = InvoiceHelper.NormalizeAndValidateCurrency(shipment.Currency),
                        NetShipmentPrice = 0.0m, PaymentStatus = PaymentStatus.Draft,
                        IssuedAt = DateTimeOffset.UtcNow, DueDate = DateTimeOffset.UtcNow.AddDays(7),
                        CreatedAt = DateTimeOffset.UtcNow, PayerType = charges.First().PayerType
                    };
                    await _unitOfWork.Invoices.AddAsync(draftInvoice);
                }
                else
                {
                    draftInvoice!.UpdatedAt = DateTimeOffset.UtcNow;
                }

                draftInvoice.Charges.Clear();
                foreach (var charge in charges)
                {
                    charge.InvoiceId = draftInvoice.Id;
                    charge.Invoice = draftInvoice;
                    draftInvoice.Charges.Add(charge);
                }

                draftInvoice.SubTotal = charges.Sum(x => x.Amount);
                draftInvoice.TaxAmount = charges.Sum(x => x.TaxAmount);
                draftInvoice.TotalAmount = draftInvoice.SubTotal + draftInvoice.TaxAmount;
                draftInvoice.Currency = shipment.Currency;
                draftInvoice.PayerType = charges.First().PayerType;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = draftInvoice.Id,
                    EntityName = nameof(Invoice).ToUpper(), Action = nameof(CreateOrUpdateDraftInvoiceAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = isCreate ? null : JsonSerializer.Serialize(draftInvoice),
                    NewValues = JsonSerializer.Serialize(draftInvoice), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Draft invoice {Id} {Action} for shipment {ShipmentId}", draftInvoice.Id, isCreate ? "created" : "updated", shipmentId);
                return Result<InvoiceResponse>.Success(_mapper.Map<InvoiceResponse>(draftInvoice), isCreate ? 201 : 200);
            });
        }

        public async Task<Result<bool>> DeleteAsync(Guid id, string userId)
        {
            _logger.LogInformation("Deleting invoice {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
                if (invoice == null) return Result<bool>.NotFound("Invoice not found.");

                if (invoice.PaymentStatus == PaymentStatus.PartiallyPaid || invoice.PaymentStatus == PaymentStatus.Paid)
                    return Result<bool>.Failure("Paid or partially paid invoices cannot be cancelled.");

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = invoice.Id,
                    EntityName = nameof(Invoice).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(invoice), NewValues = "Deleted", UserId = userId
                };

                invoice.UpdatedAt = DateTimeOffset.UtcNow;
                await _unitOfWork.AuditLog.Add(audit);
                _unitOfWork.Invoices.Delete(invoice);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice {Id} deleted", id);
                return Result<bool>.Success(true);
            });
        }

        public async Task<Result<InvoiceResponse>> CancelAsync(Guid id, string userId, bool isPrivileged, string reason)
        {
            _logger.LogInformation("Cancelling invoice {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) return Result<InvoiceResponse>.NotFound("User not found.");

                var invoice = await _unitOfWork.Invoices.GetByIdAsync(id);
                if (invoice == null) return Result<InvoiceResponse>.NotFound("Invoice not found.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(invoice.ShipmentId);
                if (shipment == null) return Result<InvoiceResponse>.NotFound("Shipment not found.");

                if (!ShipmentStatusRules.CanCancelInvoice(shipment.Status))
                    return Result<InvoiceResponse>.Failure("Cannot cancel invoice for the current shipment status.");

                if (!isPrivileged)
                {
                    if (user.CustomerProfile == null) return Result<InvoiceResponse>.NotFound("Customer profile not found.");
                    if (shipment.CustomerId != user.CustomerProfile.Id) return Result<InvoiceResponse>.Unauthorized("You do not have access to this invoice.");
                    if (invoice.PaymentStatus is not (PaymentStatus.Draft or PaymentStatus.Pending))
                        return Result<InvoiceResponse>.Failure("Only draft or pending invoices can be cancelled by customer.");
                }
                else
                {
                    if (invoice.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyPaid)
                        return Result<InvoiceResponse>.Failure("Paid or partially paid invoices cannot be cancelled.");
                }

                if (invoice.PaymentStatus == PaymentStatus.Cancelled)
                    return Result<InvoiceResponse>.Failure("Invoice is already cancelled.");

                if (string.IsNullOrWhiteSpace(reason))
                    return Result<InvoiceResponse>.Failure("Cancellation reason is required.");

                var oldInvoice = invoice;
                invoice.UpdatedAt = DateTimeOffset.UtcNow;
                invoice.PaymentStatus = PaymentStatus.Cancelled;
                invoice.CancelledAt = DateTimeOffset.UtcNow;
                invoice.CancelledByUserId = userId;
                invoice.CancellationReason = reason.Trim();

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = invoice.Id,
                    EntityName = nameof(Invoice).ToUpper(), Action = nameof(CancelAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldInvoice), NewValues = JsonSerializer.Serialize(invoice), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice {Id} cancelled", id);
                return Result<InvoiceResponse>.Success(_mapper.Map<InvoiceResponse>(invoice));
            });
        }

        public async Task<Result<InvoiceResponse>> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var context = await InvoiceHelper.GetInvoiceContextAsync(id, userId, isPrivileged, _userManager, _unitOfWork);
            return Result<InvoiceResponse>.Success(_mapper.Map<InvoiceResponse>(context.Invoice));
        }

        public async Task<Result<IReadOnlyList<InvoiceResponse>>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await InvoiceHelper.GetUserOrThrowAsync(userId, _userManager);
            var shipment = await InvoiceHelper.GetShipmentOrThrowAsync(shipmentId, _unitOfWork);

            if (!isPrivileged) InvoiceHelper.EnsureCustomerOwnsShipment(user, shipment);

            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(shipmentId);
            if (!invoices.Any()) return Result<IReadOnlyList<InvoiceResponse>>.NotFound("Invoice not found.");

            return Result<IReadOnlyList<InvoiceResponse>>.Success(_mapper.Map<IReadOnlyList<InvoiceResponse>>(invoices));
        }

        public async Task<Result<InvoiceResponse>> ConfirmAsync(Guid id, string userId)
        {
            _logger.LogInformation("Confirming invoice {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var (_, invoice, shipment) = await InvoiceHelper.GetInvoiceContextAsync(id, userId, isPrivileged: false, _userManager, _unitOfWork);

                if (!ShipmentStatusRules.CanPayInvoice(shipment.Status))
                    return Result<InvoiceResponse>.Failure("Cannot confirm invoice for the current shipment status.");

                InvoiceHelper.EnsureInvoiceCanBeConfirmed(invoice);

                var oldInvoice = invoice;
                invoice.UpdatedAt = DateTimeOffset.UtcNow;
                invoice.PaymentStatus = PaymentStatus.Pending;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = invoice.Id,
                    EntityName = nameof(Invoice).ToUpper(), Action = nameof(ConfirmAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldInvoice), NewValues = JsonSerializer.Serialize(invoice), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Invoice {Id} confirmed", id);
                return Result<InvoiceResponse>.Success(_mapper.Map<InvoiceResponse>(invoice));
            });
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(InvoiceService));
                throw;
            }
        }
    }
}
