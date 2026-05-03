using Application.Interfaces.Repositories.Shipments.Core;
using Application.Models;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class ShipmentRepository : IShipmentRepository
    {
        private readonly ApplicationDbContext _context;

        public ShipmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Shipment shipment)
        {
            await _context.Shipments.AddAsync(shipment);
        }

        public void Delete(Shipment shipment)
        {
            shipment.IsDeleted = true;
            shipment.DeletedAt = DateTimeOffset.UtcNow;
            _context.Shipments.Update(shipment);
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Shipments.AnyAsync(s => s.Id == id);
        }

        public async Task<bool> ExistsByQuoteIdAsync(Guid quoteId)
        {
            return await _context.Shipments.AnyAsync(s => s.QuoteId == quoteId);
        }

        public async Task<bool> ExistsByQuoteIdExceptAsync(Guid quoteId, Guid shipmentId)
        {
            return await _context.Shipments.AnyAsync(s => s.QuoteId == quoteId && s.Id != shipmentId);
        }

        public async Task<IReadOnlyList<Shipment>> GetAllAsync(ShipmentParameters parameters)
        {
            var query = _context.Shipments
                .AsNoTracking()
                .Include(s => s.Quote)
                .Include(s => s.Customer).ThenInclude(c => c.ApplicationUser)
                .Include(s => s.Route).ThenInclude(r => r.FromPort)
                .Include(s => s.Route).ThenInclude(r => r.ToPort)
                .Include(s => s.ContainerType)
                .Include(s => s.Carrier)
                .Where(s => !s.IsDeleted);

            if (parameters.CreatedFrom.HasValue)
                query = query.Where(c => c.CreatedAt >= parameters.CreatedFrom.Value);

            if (parameters.CreatedTo.HasValue)
                query = query.Where(c => c.CreatedAt <= parameters.CreatedTo.Value);

            if (parameters.DeliveredFrom.HasValue)
                query = query.Where(c => c.DeliveredAt >= parameters.DeliveredFrom.Value);

            if (parameters.DeliveredTo.HasValue)
                query = query.Where(c => c.DeliveredAt <= parameters.DeliveredTo.Value);

            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                var term = $"%{parameters.Search.Trim()}%";

                query = query.Where(c =>
                    EF.Functions.Like(c.Carrier.Name!, term) ||
                    EF.Functions.Like(c.Carrier.Code!, term) ||
                    EF.Functions.Like(c.Currency!, term) ||
                    EF.Functions.Like(c.ContainerType.Name!, term));
            }

            query = parameters.SortBy?.ToLower() switch
            {
                "createdat_asc" => query.OrderBy(c => c.CreatedAt),
                "createdat_desc" => query.OrderByDescending(c => c.CreatedAt),

                "deliveredat_asc" => query.OrderBy(c => c.DeliveredAt),
                "deliveredat_desc" => query.OrderByDescending(c => c.DeliveredAt),

                "currency_asc" => query.OrderBy(c => c.Currency),
                "currency_desc" => query.OrderByDescending(c => c.Currency),

                _ => query.OrderByDescending(c => c.CreatedAt)
            };

            return await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }

        public async Task<Shipment?> GetByIdAsync(Guid id)
        {
            return await _context.Shipments.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        }

        public Task<Shipment?> GetByIdWithDetailsAsync(Guid id)
        {
            return _context.Shipments.AsNoTracking()
                .Include(s => s.Quote)
                .Include(s => s.Customer).ThenInclude(c => c.ApplicationUser)
                .Include(s => s.Route)
                .Include(s => s.ContainerType)
                .Include(s => s.Carrier)
                .Include(s => s.Items)
                .Include(s => s.Charges)
                .Include(s => s.StatusHistory)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
        public Task<Shipment?> GetTrackedByIdWithDetailsAsync(Guid id)
        {
            return _context.Shipments
                .Include(s => s.Quote)
                .Include(s => s.Customer).ThenInclude(c => c.ApplicationUser)
                .Include(s => s.Route)
                .Include(s => s.ContainerType)
                .Include(s => s.Carrier)
                .Include(s => s.Items.Where(i => !i.IsDeleted))
                .Include(s => s.Charges.Where(c => !c.IsDeleted))
                .Include(s => s.StatusHistory)
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public void Update(Shipment shipment)
        {
            _context.Update(shipment);
        }
    }
}
