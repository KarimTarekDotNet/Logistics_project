using Application.Interfaces.Repositories.Shipments.Core;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class ShipmentItemRepository : IShipmentItemRepository
    {
        private readonly ApplicationDbContext _context;

        public ShipmentItemRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShipmentItem item)
        {
            await _context.ShipmentItems.AddAsync(item);
        }

        public void Delete(ShipmentItem item)
        {
            item.IsDeleted = true;
            item.DeletedAt = DateTimeOffset.UtcNow;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return _context.ShipmentItems.Any(i => i.Id == id && !i.IsDeleted);
        }

        public async Task<ShipmentItem?> GetByIdAsync(Guid id)
        {
           var item = await _context.ShipmentItems.Include(x => x.Shipment).ThenInclude(x => x.Customer)
            .FirstOrDefaultAsync(i => i.Id == id && !i.IsDeleted);
            return item;
        }

        public async Task<IReadOnlyList<ShipmentItem>> GetByShipmentIdAsync(Guid shipmentId)
        {
            var items = await _context.ShipmentItems.Where(i => i.ShipmentId == shipmentId && !i.IsDeleted).ToListAsync();
            return items;
        }

        public void Update(ShipmentItem item)
        {
            _context.ShipmentItems.Update(item);
        }
    }
}
