using Application.Models;
using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IShipmentRepository
    {
        Task<Shipment?> GetByIdAsync(Guid id);
        Task<Shipment?> GetByIdWithDetailsAsync(Guid id);
        Task<Shipment?> GetTrackedByIdWithDetailsAsync(Guid id);

        Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentParameters parameters);
        Task<IReadOnlyList<Shipment>> GetAllForUserAsync(Guid customerId, ShipmentParameters parameters);

        Task AddAsync(Shipment shipment);
        void Update(Shipment shipment);
        void Delete(Shipment shipment);

        Task<bool> ExistsAsync(Guid id);
        Task<bool> ExistsByQuoteIdAsync(Guid quoteId);
        Task<bool> ExistsByQuoteIdExceptAsync(Guid quoteId, Guid shipmentId);
        Task<int?> CountAsync();
    }
}
