using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentHoldService
    {
        Task<ShipmentResponse?> PutOnHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<ShipmentResponse?> ResumeFromHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }
}
