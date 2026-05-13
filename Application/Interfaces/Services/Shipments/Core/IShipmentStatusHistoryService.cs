using Application.DTOs.Shipments.Core;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentStatusHistoryService
    {
        Task<IReadOnlyList<ShipmentStatusHistoryResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged, QueryParameters parameters);
    }
}
