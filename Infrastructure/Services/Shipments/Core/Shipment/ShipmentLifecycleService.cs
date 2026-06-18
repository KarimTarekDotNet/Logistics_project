using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentLifecycleService : IShipmentLifecycleService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentLifecycleService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentResponse?> ApproveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.DraftBLApproved, nameof(ApproveDraftBlAsync),
                async s => { s.DraftBlApprovedAt = DateTime.UtcNow; });
        }

        public async Task<ShipmentResponse?> CloseAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.Closed, nameof(CloseAsync),
                async s => { s.ClosedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> CompleteDeliveryAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.Delivered, nameof(CompleteDeliveryAsync),
                async s => { s.DeliveredAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> ConfirmBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.BookingConfirmed, nameof(ConfirmBookingAsync),
                async s => { s.BookingConfirmedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> ConfirmClientAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.ClientConfirmed, nameof(ConfirmClientAsync),
                async s => { s.ClientConfirmedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> ConfirmPaymentAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.PaymentCompleted, nameof(ConfirmPaymentAsync),
                async s => { s.PaymentConfirmedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> MarkPaymentPendingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.PaymentPending, nameof(MarkPaymentPendingAsync),
                async s => { s.PaymentPendingAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> ReceiveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.DraftBLReceived, nameof(ReceiveDraftBlAsync),
                async s => { s.DraftBlReceivedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> ReleaseTelexAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.TelexReleased, nameof(ReleaseTelexAsync),
                async s => { s.TelexReleasedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> RequestBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.BookingRequested, nameof(RequestBookingAsync),
                async s => { s.BookingRequestedAt = DateTimeOffset.UtcNow; });
        }

        public async Task<ShipmentResponse?> SubmitShippingInstructionsAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            return await ExecuteLifecycleTransitionAsync(id, userId, isPrivileged, request,
                ShipmentStatus.ShippingInstructionsSubmitted, nameof(SubmitShippingInstructionsAsync),
                async s => { s.ShippingInstructionsSubmittedAt = DateTimeOffset.UtcNow; });
        }

        private async Task<ShipmentResponse?> ExecuteLifecycleTransitionAsync(
            Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request,
            ShipmentStatus targetStatus, string actionName,
            Func<Domain.Entities.Shipments.Shipment, Task> applyTimestamp)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork,
                    id, userId, isPrivileged, targetStatus, request.Reason);

                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }

                var oldStatus = shipment.StatusHistory
                    .OrderByDescending(h => h.ChangedAt)
                    .Skip(1)
                    .FirstOrDefault()?.ToStatus ?? shipment.Status;

                await applyTimestamp(shipment);

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = actionName.ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
                    NewValues = JsonSerializer.Serialize(new { Status = targetStatus.ToString() }),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<ShipmentResponse?>(shipment);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
