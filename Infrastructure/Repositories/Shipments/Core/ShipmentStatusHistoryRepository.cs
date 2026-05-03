using Application.Interfaces.Repositories.Shipments.Core;
using Application.Models;
using Domain.Entities.Shipments;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Shipments.Core
{
    public class ShipmentStatusHistoryRepository : IShipmentStatusHistoryRepository
    {
        private readonly ApplicationDbContext _context;

        public ShipmentStatusHistoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int> CountByShipmentIdAsync(Guid shipmentId)
        {
            var count = await _context.ShipmentStatusHistories.CountAsync(h => h.ShipmentId == shipmentId);
            return count;
        }

        public async Task<IReadOnlyList<ShipmentStatusHistory>> GetByShipmentIdAsync(Guid shipmentId, QueryParameters parameters)
        {
            var query = _context.ShipmentStatusHistories
                .Where(h => h.ShipmentId == shipmentId)
                .OrderByDescending(h => h.ChangedAt)
                .AsNoTracking();

            if (!string.IsNullOrEmpty(parameters.Search))
            {
                var searchTerm = $"%{parameters.Search.ToLower()}%";
                query = query.Where(h =>
                    EF.Functions.Like(h.ChangedBy!.ToLower(), searchTerm) ||
                    EF.Functions.Like(h.Shipment.Carrier.Name!.ToLower(), searchTerm) ||
                    EF.Functions.Like(h.Shipment.Carrier.Code!.ToLower(), searchTerm) ||
                    EF.Functions.Like(h.Shipment.Currency!.ToLower(), searchTerm) ||
                    EF.Functions.Like(h.Shipment.ContainerType.Name!.ToLower(), searchTerm) ||
                    EF.Functions.Like(h.Reason!.ToLower(), searchTerm));
            }

            if(!string.IsNullOrEmpty(parameters.SortBy))
            {
                query = parameters.SortBy.ToLower() switch
                {
                    "changedby" => query.OrderBy(h => h.ChangedBy),
                    "changedat" => query.OrderBy(h => h.ChangedAt),
                    _ => query
                };
            }

            return await query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToListAsync();
        }
    }
}
