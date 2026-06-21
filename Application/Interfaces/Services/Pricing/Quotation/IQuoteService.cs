using Application.Common;
using Application.DTOs.Pricing.Quotation;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.Quotation
{
    public interface IQuoteService
    {
        Task<Result<QuoteResponse>> CreateAsync(CreateQuoteRequest dto, string userId);
        Task<Result> DeleteAsync(Guid id, bool isAdmin, string userId);
        Task<Result<QuoteResponse>> GetByIdAsync(Guid id, string userId, bool isAdminOrStaff);
        Task<Result<IEnumerable<QuoteResponse>>> GetMyQuotesAsync(string userId, QueryParameters query);
        Task<Result<IEnumerable<QuoteResponse>>> GetByCustomerNameAsync(string customerName, QueryParameters query);
        Task<Result<IEnumerable<QuoteResponse>>> GetByRouteIdAsync(Guid routeId, QueryParameters query);
        Task<Result<IEnumerable<QuoteResponse>>> GetAllAsync(QueryParameters query);
        Task<Result<QuoteResponse>> AcceptFromUserAsync(Guid id, string userId);
        Task<Result<QuoteResponse>> RejectFromUserAsync(Guid id, string userId, string reason);
    }
}
