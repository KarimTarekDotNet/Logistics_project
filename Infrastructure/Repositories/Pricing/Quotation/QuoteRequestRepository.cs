using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Models;
using Domain.Entities.Pricing.Quotation;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Pricing.Quotation
{
    public class QuoteRequestRepository : IQuoteRequestRepository
    {
        private readonly ApplicationDbContext _context;

        public QuoteRequestRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(QuoteRequest request)
        {
            await _context.QuoteRequests.AddAsync(request);
        }

        public async Task<IEnumerable<QuoteRequest?>> GetAllAsync(QueryParameters query)
        {
            var quotes = _context.QuoteRequests
                .AsNoTracking()
                .Include(q => q.Customer)
                    .ThenInclude(c => c.ApplicationUser)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.Carrier)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.Route)
                        .ThenInclude(route => route.FromPort)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.Route)
                        .ThenInclude(route => route.ToPort)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.ContainerType);

            return await Pagination(quotes, query);
        }

        public async Task<bool> HasPendingRequestForRateAsync(Guid customerId, Guid rateId)
        {
            return await _context.QuoteRequests
                   .AnyAsync(q => q.CustomerId == customerId && q.RateId == rateId
                   && q.Status == Domain.Enums.QuoteRequestStatus.PendingReview);
        }

        public async Task<QuoteRequest?> GetMyRequestById(Guid customerId, Guid id)
        {
            return await _context.QuoteRequests
            .Include(q => q.Customer)
                .ThenInclude(c => c.ApplicationUser)
            .Include(q => q.Rate)
                .ThenInclude(r => r.Carrier)
            .Include(q => q.Rate)
                .ThenInclude(r => r.Route)
                    .ThenInclude(route => route.FromPort)
            .Include(q => q.Rate)
                .ThenInclude(r => r.Route)
                    .ThenInclude(route => route.ToPort)
            .Include(q => q.Rate)
                .ThenInclude(r => r.ContainerType)
            .FirstOrDefaultAsync(q => q.CustomerId == customerId && q.Id == id);
        }

        public async Task<QuoteRequest?> GetById(Guid id)
        {
            return await _context.QuoteRequests
            .Include(q => q.Customer)
                .ThenInclude(c => c.ApplicationUser)
            .Include(q => q.Rate)
                .ThenInclude(r => r.Carrier)
            .Include(q => q.Rate)
                .ThenInclude(r => r.Route)
                    .ThenInclude(route => route.FromPort)
            .Include(q => q.Rate)
                .ThenInclude(r => r.Route)
                    .ThenInclude(route => route.ToPort)
            .Include(q => q.Rate)
                .ThenInclude(r => r.ContainerType)
            .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<IEnumerable<QuoteRequest?>> GetMyRequests(Guid customerId, QueryParameters query)
        {
            var myQuoteRequest = _context.QuoteRequests
                .AsNoTracking()
                .Include(q => q.Customer)
                    .ThenInclude(c => c.ApplicationUser)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.Carrier)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.Route)
                        .ThenInclude(route => route.FromPort)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.Route)
                        .ThenInclude(route => route.ToPort)
                .Include(q => q.Rate)
                    .ThenInclude(r => r.ContainerType)
                .Where(q => q.CustomerId == customerId);

            return await Pagination(myQuoteRequest, query);
        }

        public void Update(QuoteRequest request)
        {
            _context.QuoteRequests.Update(request);
        }

        private async Task<IEnumerable<QuoteRequest>> Pagination(
            IQueryable<QuoteRequest> quotesQuery,
            QueryParameters query)
        {
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                var searchTerm = $"%{search}%";

                quotesQuery = quotesQuery.Where(q =>
                    EF.Functions.Like(q.Notes ?? "", searchTerm) ||
                    EF.Functions.Like(q.RejectionReason ?? "", searchTerm) ||

                    EF.Functions.Like(q.Status.ToString(), searchTerm) ||

                    EF.Functions.Like(q.Rate.Carrier.Name ?? "", searchTerm) ||
                    EF.Functions.Like(q.Rate.Carrier.Code ?? "", searchTerm) ||

                    EF.Functions.Like(q.Rate.Route.FromPort.Code ?? "", searchTerm) ||
                    EF.Functions.Like(q.Rate.Route.ToPort.Code ?? "", searchTerm) ||

                    EF.Functions.Like(q.Rate.ContainerType.Name ?? "", searchTerm)
                );
            }

            quotesQuery = query.SortBy?.ToLower() switch
            {
                "createdat" => quotesQuery.OrderByDescending(q => q.CreatedAt),
                "createdat_asc" => quotesQuery.OrderBy(q => q.CreatedAt),

                "status" => quotesQuery.OrderBy(q => q.Status),
                "status_desc" => quotesQuery.OrderByDescending(q => q.Status),

                "grossweight" => quotesQuery.OrderBy(q => q.RequestedGrossWeightKg),
                "grossweight_desc" => quotesQuery.OrderByDescending(q => q.RequestedGrossWeightKg),

                "volume" => quotesQuery.OrderBy(q => q.RequestedVolumeCbm),
                "volume_desc" => quotesQuery.OrderByDescending(q => q.RequestedVolumeCbm),

                _ => quotesQuery.OrderByDescending(q => q.CreatedAt)
            };

            return await quotesQuery
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();
        }
    }
}
