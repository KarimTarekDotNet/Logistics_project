using Application.Common;
using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentCancellationService
    {
        Task<Result<ShipmentResponse>> CancelAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }
}
