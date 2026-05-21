using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IShipmentChargeRepository
    {
        Task<ShipmentCharge?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ShipmentCharge>> GetByShipmentIdAsync(Guid shipmentId);
        Task AddAsync(ShipmentCharge charge);
        Task AddRangeAsync(List<ShipmentCharge> charges);
        void Update(ShipmentCharge charge);
        void Delete(ShipmentCharge charge);
        Task<bool> ExistsAsync(Guid id);
    }
}
