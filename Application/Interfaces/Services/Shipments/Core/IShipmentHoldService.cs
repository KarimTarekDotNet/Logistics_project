using Application.Common;
using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentHoldService
    {
        Task<Result<ShipmentResponse>> PutOnHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
        Task<Result<ShipmentResponse>> ResumeFromHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }
}
