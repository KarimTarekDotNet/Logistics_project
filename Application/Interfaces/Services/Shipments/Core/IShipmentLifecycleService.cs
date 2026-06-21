using Application.Common;
using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentLifecycleService
    {
        Task<Result<ShipmentResponse>> ConfirmClientAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> RequestBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> ConfirmBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> SubmitShippingInstructionsAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> ReceiveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> ApproveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> MarkPaymentPendingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> ConfirmPaymentAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> ReleaseTelexAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> CompleteDeliveryAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> CloseAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }
}
