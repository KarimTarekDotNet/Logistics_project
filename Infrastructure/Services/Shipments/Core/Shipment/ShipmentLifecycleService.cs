using Application.Common;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentLifecycleService : IShipmentLifecycleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentLifecycleService> _logger;

        public ShipmentLifecycleService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ShipmentLifecycleService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public Task<Result<ShipmentResponse>> ApproveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.DraftBLApproved, nameof(ApproveDraftBlAsync),
                s => { s.DraftBlApprovedAt = DateTime.UtcNow; return Task.CompletedTask; });

        public async Task<Result<ShipmentResponse>> CloseAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                return Result<ShipmentResponse>.Failure("Shipment cannot proceed before all invoices are paid.");
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.Closed, nameof(CloseAsync),
                s => { s.ClosedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });
        }

        public async Task<Result<ShipmentResponse>> CompleteDeliveryAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                return Result<ShipmentResponse>.Failure("Shipment cannot proceed before all invoices are paid.");
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.Delivered, nameof(CompleteDeliveryAsync),
                s => { s.DeliveredAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });
        }

        public Task<Result<ShipmentResponse>> ConfirmBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.BookingConfirmed, nameof(ConfirmBookingAsync),
                s => { s.BookingConfirmedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });

        public Task<Result<ShipmentResponse>> ConfirmClientAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.ClientConfirmed, nameof(ConfirmClientAsync),
                s => { s.ClientConfirmedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });

        public async Task<Result<ShipmentResponse>> ConfirmPaymentAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                return Result<ShipmentResponse>.Failure("Shipment cannot proceed before all invoices are paid.");
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.PaymentCompleted, nameof(ConfirmPaymentAsync),
                s => { s.PaymentConfirmedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });
        }

        public Task<Result<ShipmentResponse>> MarkPaymentPendingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.PaymentPending, nameof(MarkPaymentPendingAsync),
                s => { s.PaymentPendingAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });

        public Task<Result<ShipmentResponse>> ReceiveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.DraftBLReceived, nameof(ReceiveDraftBlAsync),
                s => { s.DraftBlReceivedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });

        public async Task<Result<ShipmentResponse>> ReleaseTelexAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                return Result<ShipmentResponse>.Failure("Shipment cannot proceed before all invoices are paid.");
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.TelexReleased, nameof(ReleaseTelexAsync),
                s => { s.TelexReleasedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });
        }

        public Task<Result<ShipmentResponse>> RequestBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.BookingRequested, nameof(RequestBookingAsync),
                s => { s.BookingRequestedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });

        public Task<Result<ShipmentResponse>> SubmitShippingInstructionsAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
            => ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request, ShipmentStatus.ShippingInstructionsSubmitted, nameof(SubmitShippingInstructionsAsync),
                s => { s.ShippingInstructionsSubmittedAt = DateTimeOffset.UtcNow; return Task.CompletedTask; });

        private async Task<Result<ShipmentResponse>> ExecuteLifecycleTransitionAsync(
            Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request,
            ShipmentStatus targetStatus, string actionName,
            Func<Domain.Entities.Shipments.Shipment, Task> applyTimestamp)
        {
            _logger.LogInformation("Shipment {Id} transitioning to {Status} by user {UserId}", id, targetStatus, userId);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, targetStatus, request.Reason);
                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<ShipmentResponse>.NotFound("Shipment not found.");
                }

                var oldStatus = shipment.StatusHistory.OrderByDescending(h => h.ChangedAt).Skip(1).FirstOrDefault()?.ToStatus ?? shipment.Status;
                await applyTimestamp(shipment);

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(), Action = actionName.ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
                    NewValues = JsonSerializer.Serialize(new { Status = targetStatus.ToString() }), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Shipment {Id} transitioned to {Status}", id, targetStatus);
                return Result<ShipmentResponse>.Success(_mapper.Map<ShipmentResponse>(shipment));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Lifecycle transition failed for shipment {Id} -> {Status}", id, targetStatus);
                throw;
            }
        }
    }
}
