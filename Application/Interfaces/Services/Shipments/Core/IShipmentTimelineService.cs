using Application.DTOs.Shipments.Core;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentTimelineService
    {
        Task<IReadOnlyCollection<ShipmentTimelineItemResponse>> GetShipmentTimelineAsync(Guid shipmentId, QueryParameters query,
        string userId, bool isPrivileged);
    }
}