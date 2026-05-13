using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IShipmentDocumentRepository
    {
        Task<ShipmentDocument?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ShipmentDocument>> GetByShipmentIdAsync(Guid shipmentId);
        Task AddAsync(ShipmentDocument charge);
        void Update(ShipmentDocument charge);
        void Delete(ShipmentDocument charge);
        Task<bool> ExistsAsync(Guid id);
    }
}
