using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentLifecycleService
    {
        Task<ShipmentResponse?> ConfirmClientAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> RequestBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> ConfirmBookingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> SubmitShippingInstructionsAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> ReceiveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> ApproveDraftBlAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> MarkPaymentPendingAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> ConfirmPaymentAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> ReleaseTelexAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> CompleteDeliveryAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> CloseAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }
}
