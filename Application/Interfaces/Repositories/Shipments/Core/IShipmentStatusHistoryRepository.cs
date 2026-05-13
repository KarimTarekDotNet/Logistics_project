using Application.Models;
using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IShipmentStatusHistoryRepository
    {
        Task<IReadOnlyList<ShipmentStatusHistory>> GetByShipmentIdAsync(Guid shipmentId, QueryParameters parameters);
        Task<IReadOnlyList<ShipmentStatusHistory>> GetByShipmentIdForTimelineAsync(Guid shipmentId);

        Task<int> CountByShipmentIdAsync(Guid shipmentId);
    }
}
