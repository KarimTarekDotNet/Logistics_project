using Application.Models;
using Domain.Entities.Pricing.Quotation;

namespace Application.Interfaces.Repositories.Pricing.Quotation
{
    public interface IQuoteRequestRepository
    {
        Task AddAsync(QuoteRequest request);
        void Update(QuoteRequest request);
        Task<IEnumerable<QuoteRequest?>> GetMyRequests(Guid customerId, QueryParameters query);
        Task<QuoteRequest?> GetMyRequestById(Guid customerId, Guid id);
        Task<IEnumerable<QuoteRequest?>> GetAllAsync(QueryParameters query);
        Task<QuoteRequest?> GetById(Guid id);
        Task<bool> HasPendingRequestForRateAsync(Guid customerId, Guid rateId);
    }
}