using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Models;
using Domain.Entities.Pricing.PricingEngine;
using Infrastructure.Data.Database;
using Infrastructure.Repositories.Patterns;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.PricingEngine
{
    public class RateRepository : GenericRepository<Rate>, IRateRepository
    {
        private readonly ApplicationDbContext _context;
        public RateRepository(ApplicationDbContext context) : base(context) {
        
            _context = context;
        }

        public async Task<Rate?> GetById(Guid Id)
        {
            return await _context.Rates.FirstOrDefaultAsync(r => r.Id == Id && r.IsActive && !r.IsDeleted);
        }
        public async Task<Rate?> GetByIdWithDetailsAsync(Guid Id)
        {
            return await _context.Rates
                .Include(r => r.Carrier)
                .Include(r => r.ContainerType)
                .Include(r => r.Route)
                    .ThenInclude(r => r.FromPort)
                .Include(r => r.Route)
                    .ThenInclude(r => r.ToPort)
                .FirstOrDefaultAsync(r => r.Id == Id && r.IsActive && !r.IsDeleted);
        }

        public async Task<IEnumerable<Rate>> SearchAsync(RateParameters query)
        {
            var ratesQuery = _context.Rates
                .AsNoTracking()
                .Include(r => r.Carrier)
                .Include(r => r.Route).ThenInclude(r => r.FromPort)
                .Include(r => r.Route).ThenInclude(r => r.ToPort)
                .Include(r => r.ContainerType)
                .Where(x => !x.IsDeleted);

            return await Pagination(ratesQuery, query);
        }

        public async Task<bool> ExistsActiveRateAsync(Guid carrierId, Guid routeId, Guid containerTypeId)
        {
            var now = DateTimeOffset.UtcNow;

            return await _context.Rates.AnyAsync(r =>
                r.CarrierId == carrierId &&
                r.RouteId == routeId &&
                r.ContainerTypeId == containerTypeId &&
                r.IsActive &&
                !r.IsDeleted &&
                r.ValidFrom <= now &&
                r.ValidTo >= now
            );
        }

        public async Task<int?> CountAsync()
        {
            return await _context.Rates.CountAsync();
        }

        public async Task<IEnumerable<Rate>> GetAvailableRatesByCarrierRouteAndContainerTypeAsync(Guid carrierId, Guid routeId, Guid containerTypeId,
        DateTimeOffset validFrom, DateTimeOffset validTo)
        {
            return await _context.Rates
                .Where(r => r.IsActive
                    && !r.IsDeleted
                    && r.CarrierId == carrierId
                    && r.RouteId == routeId
                    && r.ContainerTypeId == containerTypeId
                    && r.ValidFrom == validFrom && r.ValidTo == validTo)
                .ToListAsync();
        }

        public IQueryable<Rate> GetRatesByRouteAndContainerTypeQuery(Guid routeId, Guid containerTypeId, string currency)
        {
            var now = DateTimeOffset.UtcNow;
            return _context.Rates
                .Where(r => !r.IsDeleted
                    && r.RouteId == routeId
                    && r.ContainerTypeId == containerTypeId
                    && EF.Functions.Like(currency, r.Currency)
                    && r.ValidFrom <= now && r.ValidTo >= now);
        }
        public IQueryable<Rate> GetRatesByRouteAndContainerTypeQueryForRecommendation(Guid routeId, Guid containerTypeId, string currency, decimal? maxPrice)
        {
            var now = DateTimeOffset.UtcNow;
            return _context.Rates
                .Include(r => r.Carrier)
                .Include(r => r.Route)
                    .ThenInclude(route => route.FromPort)
                .Include(r => r.Route)
                    .ThenInclude(route => route.ToPort)
                .Include(r => r.ContainerType)
                .Where(r => !r.IsDeleted
                    && r.IsActive
                    && r.RouteId == routeId
                    && r.ContainerTypeId == containerTypeId
                    && r.Currency == currency
                    && r.ValidFrom <= now && r.ValidTo >= now
                    && (!maxPrice.HasValue || r.Price <= maxPrice.Value));
        }

        public async Task<IEnumerable<Rate>> GetByCarrierRouteAndContainerTypeAsync(Guid carrierId, Guid routeId, Guid containerTypeId)
        {
            return await _context.Rates
                .Where(r =>
                    r.CarrierId == carrierId &&
                    r.RouteId == routeId &&
                    r.ContainerTypeId == containerTypeId &&
                    !r.IsDeleted)
                .ToListAsync();
        }

        private async Task<IEnumerable<Rate>> Pagination(IQueryable<Rate> ratesQuery, RateParameters query)
        {
            // Filters
            if (query.OnlyActive.HasValue)
            {
                if(query.OnlyActive.Value)
                    ratesQuery = ratesQuery.Where(r => r.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(query.CarrierName))
            {
                var carrierName = $"%{query.CarrierName.Trim()}%";
                ratesQuery = ratesQuery.Where(r => EF.Functions.Like(r.Carrier.Name, carrierName));
            }

            if (!string.IsNullOrWhiteSpace(query.ContainerTypeName))
            {
                var containerTypeName = $"%{query.ContainerTypeName.Trim()}%";
                ratesQuery = ratesQuery.Where(r => EF.Functions.Like(r.ContainerType.Name, containerTypeName));
            }

            if (!string.IsNullOrWhiteSpace(query.FromPortName))
            {
                var fromPortName = $"%{query.FromPortName.Trim()}%";
                ratesQuery = ratesQuery.Where(r => EF.Functions.Like(r.Route.FromPort.Name, fromPortName) ||
                EF.Functions.Like(r.Route.FromPort.Code, fromPortName) ||
                EF.Functions.Like(r.Route.FromPort.Country, fromPortName));
            }

            if (!string.IsNullOrWhiteSpace(query.ToPortName))
            {
                var toPortName = $"%{query.ToPortName.Trim()}%";
                ratesQuery = ratesQuery.Where(r =>
                EF.Functions.Like(r.Route.ToPort.Name, toPortName) ||
                EF.Functions.Like(r.Route.ToPort.Code, toPortName) ||
                EF.Functions.Like(r.Route.ToPort.Country, toPortName));
            }

            if (query.MinPrice.HasValue)
            {
                ratesQuery = ratesQuery.Where(r => r.Price >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                ratesQuery = ratesQuery.Where(r => r.Price <= query.MaxPrice.Value);
            }

            if (!string.IsNullOrWhiteSpace(query.Currency))
            {
                var currency = query.Currency.Trim().ToUpper();
                ratesQuery = ratesQuery.Where(r => r.Currency.ToUpper() == currency);
            }

            if (query.ValidFrom.HasValue)
            {
                ratesQuery = ratesQuery.Where(r => r.ValidFrom >= query.ValidFrom.Value);
            }

            if (query.ValidTo.HasValue)
            {
                ratesQuery = ratesQuery.Where(r => r.ValidTo <= query.ValidTo.Value);
            }

            if (query.CreatedFrom.HasValue)
            {
                ratesQuery = ratesQuery.Where(r => r.CreatedAt >= query.CreatedFrom.Value);
            }

            if (query.CreatedTo.HasValue)
            {
                ratesQuery = ratesQuery.Where(r => r.CreatedAt <= query.CreatedTo.Value);
            }

            if(query.OnlyCurrentlyValid.HasValue && query.OnlyCurrentlyValid.Value)
            {
                var now = DateTimeOffset.UtcNow;
                ratesQuery = ratesQuery.Where(r => r.IsActive && r.ValidFrom <= now && r.ValidTo >= now);
            }

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var term = query.Search.Trim();
                var searchTerm = $"%{term}%";

                ratesQuery = ratesQuery.Where(r =>
                    EF.Functions.Like(r.Carrier.Name, searchTerm) ||
                    EF.Functions.Like(r.Route.FromPort.Name, searchTerm) ||
                    EF.Functions.Like(r.Route.ToPort.Name, searchTerm) ||
                    EF.Functions.Like(r.Route.FromPort.Code, searchTerm) ||
                    EF.Functions.Like(r.Route.ToPort.Code, searchTerm) ||
                    EF.Functions.Like(r.Route.FromPort.Country, searchTerm) ||
                    EF.Functions.Like(r.Route.ToPort.Country, searchTerm) ||
                    EF.Functions.Like(r.ContainerType.Name, searchTerm) ||
                    EF.Functions.Like(r.Currency, searchTerm));
            }

            // Sorting
            ratesQuery = query.SortBy?.ToLower() switch
            {
                "price_asc" => ratesQuery.OrderBy(r => r.Price),
                "price_desc" => ratesQuery.OrderByDescending(r => r.Price),

                "createdat_asc" => ratesQuery.OrderBy(r => r.CreatedAt),
                "createdat_desc" => ratesQuery.OrderByDescending(r => r.CreatedAt),

                "validto_asc" => ratesQuery.OrderBy(r => r.ValidTo),
                "validto_desc" => ratesQuery.OrderByDescending(r => r.ValidTo),

                "validfrom_asc" => ratesQuery.OrderBy(r => r.ValidFrom),
                "validfrom_desc" => ratesQuery.OrderByDescending(r => r.ValidFrom),

                "name_asc" => ratesQuery.OrderBy(r => r.Carrier.Name),
                "name_desc" => ratesQuery.OrderByDescending(r => r.Carrier.Name),

                "type_asc" => ratesQuery.OrderBy(r => r.ContainerType.Name),
                "type_desc" => ratesQuery.OrderByDescending(r => r.ContainerType.Name),

                "route_asc" => ratesQuery.OrderBy(r => r.Route.FromPort.Name).ThenBy(r => r.Route.ToPort.Name),
                "route_desc" => ratesQuery.OrderByDescending(r => r.Route.FromPort.Name).ThenByDescending(r => r.Route.ToPort.Name),

                _ => ratesQuery.OrderBy(r => r.Price)
            };

            return await ratesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
