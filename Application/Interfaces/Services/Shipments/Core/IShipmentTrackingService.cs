using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentTrackingService
    {
        Task<ShipmentResponse?> UpdateTrackingAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentTrackingRequest request);
    }
}
