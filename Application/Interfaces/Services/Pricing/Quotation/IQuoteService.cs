using Application.DTOs.Pricing.Quotation;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.Quotation
{
    public interface IQuoteService
    {
        Task<QuoteResponse> CreateAsync(CreateQuoteRequest dto, string userId);

        Task DeleteAsync(Guid id, bool isAdmin, string userId);

        Task<QuoteResponse?> GetByIdAsync(Guid id, string userId, bool isAdminOrStaff);

        Task<IEnumerable<QuoteResponse>> GetMyQuotesAsync(string userId, QueryParameters query);

        Task<IEnumerable<QuoteResponse>> GetByCustomerNameAsync(string customerName, QueryParameters query);

        Task<IEnumerable<QuoteResponse>> GetByRouteIdAsync(Guid routeId, QueryParameters query);

        Task<IEnumerable<QuoteResponse>> GetAllAsync(QueryParameters query);

        Task<QuoteResponse> AcceptFromUserAsync(Guid id, string userId);

        Task<QuoteResponse> RejectFromUserAsync(Guid id, string userId, string reason);
    }
}