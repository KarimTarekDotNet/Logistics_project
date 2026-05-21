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
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, 
                id, userId, isPrivileged, ShipmentStatus.DraftBLApproved, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.DraftBlApprovedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> CloseAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.Closed, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.ClosedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> CompleteDeliveryAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.Delivered, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.DeliveredAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> ConfirmBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.BookingConfirmed, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.BookingConfirmedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> ConfirmClientAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.ClientConfirmed, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.ClientConfirmedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> ConfirmPaymentAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.PaymentCompleted, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.PaymentConfirmedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> MarkPaymentPendingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.PaymentPending, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.PaymentPendingAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> ReceiveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.DraftBLReceived, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.DraftBlReceivedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> ReleaseTelexAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var invoices = await _unitOfWork.Invoices.GetByShipmentIdAsync(id);
            if (!invoices.Any() || !invoices.All(x => x.PaymentStatus == PaymentStatus.Paid))
                throw new BusinessRuleException("Shipment cannot proceed before all invoices are paid.");

            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.TelexReleased, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.TelexReleasedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> RequestBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.BookingRequested, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.BookingRequestedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }

        public async Task<ShipmentResponse?> SubmitShippingInstructionsAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, 
                id, userId, isPrivileged, ShipmentStatus.ShippingInstructionsSubmitted, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.ShippingInstructionsSubmittedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }
    }
}
