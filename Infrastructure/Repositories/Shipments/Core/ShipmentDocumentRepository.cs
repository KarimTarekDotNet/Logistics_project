using Application.Interfaces.Repositories.Shipments.Core;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class ShipmentDocumentRepository : IShipmentDocumentRepository
    {
        private readonly ApplicationDbContext _context;

        public ShipmentDocumentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShipmentDocument document)
        {
            await _context.ShipmentDocuments.AddAsync(document);
        }

        public void Delete(ShipmentDocument document)
        {
            document.IsDeleted = true;
            document.DeletedAt = DateTimeOffset.UtcNow;
        }

        public void Update(ShipmentDocument document)
        {
            _context.ShipmentDocuments.Update(document);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ShipmentDocuments.AnyAsync(x => x.Id == id);
        }

        public async Task<ShipmentDocument?> GetByIdAsync(Guid id)
        {
            return await _context.ShipmentDocuments.Include(x => x.Shipment).ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<IReadOnlyList<ShipmentDocument>> GetByShipmentIdAsync(Guid shipmentId)
        {
            return await _context.ShipmentDocuments.Include(x => x.Shipment).ThenInclude(x => x.Customer)
            .Where(x => x.ShipmentId == shipmentId).ToListAsync();
        }
    }
}
