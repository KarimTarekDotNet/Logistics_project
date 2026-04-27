using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Models;
using Domain.Entities.Pricing.Quotation;
using Infrastructure.Data;
using Infrastructure.Repositories.Patterns;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.Quotation
{
    public class QuoteRepository : GenericRepository<Quote>, IQuoteRepository
    {
        private readonly ApplicationDbContext _context;

        public QuoteRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Quote?> GetWithItemsAsync(Guid id)
        {
            return await _context.Quotes
                .AsNoTracking()
                .Include(q => q.Items)
                .Include(q => q.Route).ThenInclude(r => r.FromPort)
                .Include(q => q.Route).ThenInclude(r => r.ToPort)
                .Include(q => q.ContainerType)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<Quote>> GetAllWithDetailsAsync(QueryParameters query)
        {
            var quotesQuery = _context.Quotes
                .AsNoTracking()
                .Include(x => x.Items)
                .Include(q => q.Route).ThenInclude(r => r.FromPort)
                .Include(q => q.Route).ThenInclude(r => r.ToPort)
                .Include(q => q.ContainerType)
                .AsQueryable();
            return await Pagination(quotesQuery, query);
        }

        public async Task<IEnumerable<Quote>> GetByCustomerNameAsync(string customerName, QueryParameters query)
        {
            var quotesQuery = _context.Quotes
                .AsNoTracking()
                .Include(x => x.Items)
                .Include(q => q.Route).ThenInclude(r => r.FromPort)
                .Include(q => q.Route).ThenInclude(r => r.ToPort)
                .Include(q => q.ContainerType)
                .Where(q => EF.Functions.Like(q.CustomerName, $"%{customerName}%"))
                .AsQueryable();

            return await Pagination(quotesQuery, query);
        }

        public async Task<IEnumerable<Quote>> GetByRouteAsync(Guid routeId, QueryParameters query)
        {
            var quotesQuery = _context.Quotes
                .AsNoTracking()
                .Include(x => x.Items)
                .Include(q => q.Route).ThenInclude(r => r.FromPort)
                .Include(q => q.Route).ThenInclude(r => r.ToPort)
                .Include(q => q.ContainerType)
                .Where(q => q.RouteId == routeId)
                .AsQueryable();

            return await Pagination(quotesQuery, query);
        }

        private async Task<IEnumerable<Quote>> Pagination(IQueryable<Quote> quotesQuery, QueryParameters query)
        {
            if (!string.IsNullOrEmpty(query.Search))
            {
                var searchTerm = $"%{query.Search}%";
                quotesQuery = quotesQuery
                    .Where(q => EF.Functions.Like(q.CustomerName, searchTerm)
                    || EF.Functions.Like(q.Route.FromPort.Name, searchTerm)
                    || EF.Functions.Like(q.Route.ToPort.Name, searchTerm)
                    || EF.Functions.Like(q.ContainerType.Name, searchTerm));
            }

            quotesQuery = query.SortBy?.ToLower() switch
            {
                "customer" => quotesQuery.OrderBy(q => q.CustomerName),
                "price" => quotesQuery.OrderBy(q => q.FinalPrice),
                "price_desc" => quotesQuery.OrderByDescending(q => q.FinalPrice),
                "createdat" => quotesQuery.OrderByDescending(q => q.CreatedAt),
                "createdat_asc" => quotesQuery.OrderBy(q => q.CreatedAt),
                _ => quotesQuery.OrderByDescending(q => q.CreatedAt)
            };

            return await quotesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
