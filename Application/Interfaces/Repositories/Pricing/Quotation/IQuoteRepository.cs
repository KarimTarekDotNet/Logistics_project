using Application.Interfaces.Repositories.Patterns;
using Application.Models;
using Domain.Entities.Pricing.Quotation;

namespace Application.Interfaces.Repositories.Pricing.Quotation
{
    public interface IQuoteRepository : IGenericRepository<Quote>
    {
        Task<Quote?> GetWithItemsAsync(Guid id);
        Task<IEnumerable<Quote>> GetByCustomerNameAsync(string customerName, QueryParameters query);
        Task<IEnumerable<Quote>> GetByRouteAsync(Guid routeId, QueryParameters query);
        Task<IEnumerable<Quote>> GetAllWithDetailsAsync(QueryParameters query);
        Task<IEnumerable<Quote>> GetByCustomerIdAsync(Guid customerId, QueryParameters query);
        Task<Quote?> GetByIdAndCustomerIdAsync(Guid quoteId, Guid customerId);
    }
}