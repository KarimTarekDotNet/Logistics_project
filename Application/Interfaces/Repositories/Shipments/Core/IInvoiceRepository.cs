using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Invoice>> GetByShipmentIdAsync(Guid shipmentId);
        Task<IReadOnlyList<Invoice>> GetByShipmentChargeIdAsync(Guid shipmentId);
        Task AddAsync(Invoice invoice);
        void Update(Invoice invoice);
        void Delete(Invoice invoice);
        Task<bool> ExistsAsync(Guid id);
    }
}