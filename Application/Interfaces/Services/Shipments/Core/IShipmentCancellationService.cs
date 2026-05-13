using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentCancellationService
    {
        Task<ShipmentResponse?> CancelAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }
}
