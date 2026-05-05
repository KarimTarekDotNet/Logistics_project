using Application.DTOs.Pricing.Quotation;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.Quotation
{
    public interface IQuoteService
    {
        Task<QuoteResponse?> GetByIdAsync(Guid id, string userId, bool isAdminOrStaff);
        Task<IEnumerable<QuoteResponse>> GetAllAsync(QueryParameters query);
        Task<IEnumerable<QuoteResponse>> GetByCustomerNameAsync(string customerName, QueryParameters query);
        Task<IEnumerable<QuoteResponse>> GetByRouteIdAsync(Guid routeId, QueryParameters query);
        Task<QuoteResponse> CreateAsync(CreateQuoteRequest dto);
        Task DeleteAsync(Guid id);
    }
}