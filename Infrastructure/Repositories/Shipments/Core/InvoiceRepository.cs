using Application.Interfaces.Repositories.Shipments.Core;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly ApplicationDbContext _context;

        public InvoiceRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
        }

        public void Delete(Invoice invoice)
        {
            invoice.DeletedAt = DateTimeOffset.UtcNow;
            invoice.IsDeleted = true;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Invoices
                .AnyAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<Invoice?> GetByIdAsync(Guid id)
        {
            return await _context.Invoices
                .Include(x => x.Charges)
                .Include(x => x.Shipment)
                .Include(x => x.Payments)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
        }

        public async Task<IReadOnlyList<Invoice>> GetByShipmentChargeIdAsync(Guid shipmentChargeId)
        {
            return await _context.Invoices
            .Include(x => x.Charges)
            .Include(x => x.Shipment)
            .Where(x => !x.IsDeleted && x.Charges.Any(c => c.Id == shipmentChargeId))
            .ToListAsync();
        }

        public async Task<IReadOnlyList<Invoice>> GetByShipmentIdAsync(Guid shipmentId)
        {
            return await _context.Invoices
                .Include(x => x.Charges)
                .Include(x => x.Shipment)
                .Include(x => x.Payments)
                .Where(x => x.ShipmentId == shipmentId && !x.IsDeleted)
                .ToListAsync();
        }

        public void Update(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
        }
    }
}
