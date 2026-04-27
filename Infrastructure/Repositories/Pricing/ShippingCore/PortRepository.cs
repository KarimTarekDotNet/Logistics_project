using Application.Interfaces.Repositories.ShippingCore;
using Application.Models;
using Domain.Entities.ShippingCore;
using Infrastructure.Data;
using Infrastructure.Repositories.Patterns;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.ShippingCore
{
    public class PortRepository : GenericRepository<Port>, IPortRepository
    {
        private readonly ApplicationDbContext _context;
        public PortRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<Port?> GetByCodeAsync(string code)
        {
            var normalizedCode = code.Trim().ToLower();
            var port = await _context.Ports.FirstOrDefaultAsync(p => EF.Functions.Like(p.Code, normalizedCode));
            return port;
        }

        public async Task<Port?> GetByNameOrCodeAsync(string input)
        {
            var normalizedInput = input.Trim().ToLower();
            var port = await _context.Ports.FirstOrDefaultAsync(p => EF.Functions.Like(p.Name, normalizedInput)
            || EF.Functions.Like(p.Code, normalizedInput));
            return port;
        }

        public async Task<IEnumerable<Port>> GetAllAsync(QueryParameters query)
        {
            var PortsQuery = _context.Ports.AsQueryable();
            
            return await Pagination(PortsQuery, query);
        }

        public async Task<IEnumerable<Port>> GetByCountryAsync(string country, QueryParameters query)
        {
            var normalizedCountry = country.Trim().ToLower();
            var PortsQuery = _context.Ports
                .Where(p => EF.Functions.Like(p.Country, normalizedCountry));
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchTerm = $"%{query.Search}%";
                PortsQuery = PortsQuery
                    .Where(r => EF.Functions.Like(r.Name, searchTerm)
                    || EF.Functions.Like(r.Code, searchTerm)
                    || EF.Functions.Like(r.Country, searchTerm));
            }

            // Sorting
            PortsQuery = query.SortBy?.ToLower() switch
            {
                "code" => PortsQuery.OrderBy(c => c.Code),
                "country" => PortsQuery.OrderBy(c => c.Country),
                _ => PortsQuery.OrderBy(c => c.Name)
            };

            return await PortsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }

        private async Task<IEnumerable<Port>> Pagination(IQueryable<Port> PortsQuery, QueryParameters query)
        {
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchTerm = $"%{query.Search}%";
                PortsQuery = PortsQuery
                    .Where(r => EF.Functions.Like(r.Name, searchTerm)
                    || EF.Functions.Like(r.Code, searchTerm)
                    || EF.Functions.Like(r.Country, searchTerm));
            }

            // Sorting
            PortsQuery = query.SortBy?.ToLower() switch
            {
                "code" => PortsQuery.OrderBy(c => c.Code),
                "country" => PortsQuery.OrderBy(c => c.Country),
                _ => PortsQuery.OrderBy(c => c.Name)
            };

            return await PortsQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
