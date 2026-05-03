using Application.Interfaces.Repositories.ShippingCore;
using Application.Models;
using Domain.Entities.ShippingCore;
using Infrastructure.Data.Database;
using Infrastructure.Repositories.Patterns;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.ShippingCore
{
    public class RouteRepository : GenericRepository<Route>, IRouteRepository
    {
        private readonly ApplicationDbContext _context;
        public RouteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Route?> GetWithPortsAsync(Guid id)
        {
            return await _context.Routes
                .AsNoTracking()
                .Include(r => r.FromPort)
                .Include(r => r.ToPort)
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task<Route?> GetByPortsAsync(Guid fromPortId, Guid toPortId)
        {
            return await _context.Routes
                .AsNoTracking()
                .Include(r => r.FromPort)
                .Include(r => r.ToPort)
                .FirstOrDefaultAsync(r => r.FromPortId == fromPortId && r.ToPortId == toPortId);
        }

        public async Task<IEnumerable<Route>> GetByFromPortAsync(Guid fromPortId, QueryParameters query)
        {
            var routesQuery = _context.Routes
                .AsNoTracking()
                .Include(r => r.FromPort)
                .Include(r => r.ToPort)
                .Where(r => r.FromPortId == fromPortId)
                .AsQueryable();

            return await Pagination(routesQuery, query);
        }

        public async Task<IEnumerable<Route>> GetByToPortAsync(Guid toPortId, QueryParameters query)
        {
            var routesQuery = _context.Routes
                .AsNoTracking()
                .Include(r => r.FromPort)
                .Include(r => r.ToPort)
                .Where(r => r.ToPortId == toPortId)
                .AsQueryable();

            return await Pagination(routesQuery, query);
        }

        public async Task<IEnumerable<Route>> GetAllAsync(QueryParameters query)
        {
            var routesQuery = _context.Routes
                .AsNoTracking()
                .Include(r => r.FromPort)
                .Include(r => r.ToPort)
                .AsQueryable();
            return await Pagination(routesQuery, query);
        }

        private async Task<IEnumerable<Route>> Pagination(IQueryable<Route> routesQuery, QueryParameters query)
        {
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchTerm = $"%{query.Search}%";
                routesQuery = routesQuery
                    .Where(r => EF.Functions.Like(r.FromPort.Name, searchTerm)
                    || EF.Functions.Like(r.FromPort.Code, searchTerm)
                    || EF.Functions.Like(r.ToPort.Name, searchTerm)
                    || EF.Functions.Like(r.ToPort.Code, searchTerm));
            }

            routesQuery = query.SortBy?.ToLower() switch
            {
                "from" => routesQuery.OrderBy(r => r.FromPort.Name),
                "to" => routesQuery.OrderBy(r => r.ToPort.Name),
                _ => routesQuery.OrderBy(r => r.FromPort.Name)
            };

            return await routesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
