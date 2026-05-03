using Application.Interfaces.Repositories.ShippingCore;
using Application.Models;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.ShippingCore;
using Infrastructure.Data.Database;
using Infrastructure.Repositories.Patterns;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.ShippingCore
{
    public class CarrierRepository
    : GenericRepository<Carrier>, ICarrierRepository
    {
        private readonly ApplicationDbContext _context;

        public CarrierRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Carrier?> GetByNameOrCodeAsync(string input)
        {
            var normalizedInput = input.Trim().ToLower();
            return await _context.Carriers.Include(x => x.Rates)
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Name, normalizedInput) ||
                EF.Functions.Like(c.Code, normalizedInput));
        }

        public async Task<IEnumerable<Carrier>> GetAllAsync(QueryParameters query)
        {
            var carriersQuery = _context.Carriers
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Include(c => c.Rates)
                .AsQueryable();

            return await Pagination(carriersQuery, query);
        }

        private async Task<IEnumerable<Carrier>> Pagination(IQueryable<Carrier> carriersQuery, QueryParameters query)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var searchTerm = $"%{query.Search.Trim()}%";

                carriersQuery = carriersQuery.Where(c =>
                    EF.Functions.Like(c.Name, searchTerm) ||
                    EF.Functions.Like(c.Code, searchTerm) ||
                    c.Rates.Any(r =>
                        EF.Functions.Like(r.Route.FromPort.Name, searchTerm) ||
                        EF.Functions.Like(r.Route.ToPort.Name, searchTerm) ||
                        EF.Functions.Like(r.Route.FromPort.Country, searchTerm) ||
                        EF.Functions.Like(r.Route.ToPort.Country, searchTerm) ||
                    EF.Functions.Like(r.ContainerType.Name, searchTerm) ||
                    EF.Functions.Like(r.Currency, searchTerm)));
            }

            // Sorting
            carriersQuery = query.SortBy?.ToLower() switch
            {
                "price_asc" => carriersQuery.OrderBy(c => c.Rates.Min(r => r.Price)),
                "price_desc" => carriersQuery.OrderByDescending(c => c.Rates.Max(r => r.Price)),

                "createdat_asc" => carriersQuery.OrderBy(c => c.Rates.Min(r => r.CreatedAt)),
                "createdat_desc" => carriersQuery.OrderByDescending(c => c.Rates.Max(r => r.CreatedAt)),

                "validto_asc" => carriersQuery.OrderBy(c => c.Rates.Min(r => r.ValidTo)),
                "validto_desc" => carriersQuery.OrderByDescending(c => c.Rates.Max(r => r.ValidTo)),

                "validfrom_asc" => carriersQuery.OrderBy(c => c.Rates.Min(r => r.ValidFrom)),
                "validfrom_desc" => carriersQuery.OrderByDescending(c => c.Rates.Max(r => r.ValidFrom)),

                "name_asc" => carriersQuery.OrderBy(c => c.Name),
                "name_desc" => carriersQuery.OrderByDescending(c => c.Name),

                "type_asc" => carriersQuery.OrderBy(c => c.Rates.Min(r => r.ContainerType.Name)),
                "type_desc" => carriersQuery.OrderByDescending(c => c.Rates.Max(r => r.ContainerType.Name)),

                "route_asc" => carriersQuery.OrderBy(c => c.Rates.Min(r => r.Route.FromPort.Name)).ThenBy(c => c.Rates.Min(r => r.Route.ToPort.Name)),
                "route_desc" => carriersQuery.OrderByDescending(c => c.Rates.Max(r => r.Route.FromPort.Name)).ThenByDescending(c => c.Rates.Max(r => r.Route.ToPort.Name)),

                _ => carriersQuery.OrderBy(c => c.Name)
            };

            return await carriersQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
