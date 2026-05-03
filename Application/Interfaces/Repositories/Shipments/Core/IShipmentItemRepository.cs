using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IShipmentItemRepository
    {
        Task<ShipmentItem?> GetByIdAsync(Guid id);

        Task<IReadOnlyList<ShipmentItem>> GetByShipmentIdAsync(Guid shipmentId);

        Task AddAsync(ShipmentItem item);

        void Update(ShipmentItem item);

        void Delete(ShipmentItem item);

        Task<bool> ExistsAsync(Guid id);
    }
}
