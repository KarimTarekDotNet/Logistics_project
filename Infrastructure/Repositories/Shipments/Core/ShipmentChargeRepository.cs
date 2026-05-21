using Application.Interfaces.Repositories.Shipments.Core;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class ShipmentChargeRepository : IShipmentChargeRepository
    {
        private readonly ApplicationDbContext _context;

        public ShipmentChargeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ShipmentCharge charge)
        {
            await _context.ShipmentCharges.AddAsync(charge);
        }
        public async Task AddRangeAsync(List<ShipmentCharge> charges)
        {
            await _context.ShipmentCharges.AddRangeAsync(charges);
        }

        public void Delete(ShipmentCharge charge)
        {
            charge.IsDeleted = true;
            charge.DeletedAt = DateTimeOffset.UtcNow;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.ShipmentCharges.AnyAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<ShipmentCharge?> GetByIdAsync(Guid id)
        {
            return await _context.ShipmentCharges
            .Include(x => x.Shipment).FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
        }

        public async Task<IReadOnlyList<ShipmentCharge>> GetByShipmentIdAsync(Guid shipmentId)
        {
            var charges = await _context.ShipmentCharges.Include(x => x.Shipment).Where(c => c.ShipmentId == shipmentId && !c.IsDeleted).ToListAsync();
            return charges;
        }

        public void Update(ShipmentCharge charge)
        {
            _context.ShipmentCharges.Update(charge);
        }
    }
}
