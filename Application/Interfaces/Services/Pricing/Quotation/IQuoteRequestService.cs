using Application.DTOs.Pricing.Quotation;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.Quotation
{
    public interface IQuoteRequestService
    {
        Task<QuoteRequestResponse> CreateFromRateAsync(CreateQuoteRequestFromRate request, string userId);

        Task<IEnumerable<QuoteRequestResponse>> GetMyRequestsAsync(string userId, QueryParameters query);
        Task<QuoteRequestResponse> GetByIdAsync(Guid id);

        Task<IEnumerable<QuoteRequestResponse>> GetAllAsync(string userId, QueryParameters query);

        Task<QuoteRequestResponse> ApproveAsync(Guid requestId, string userId);

        Task<QuoteRequestResponse> RejectAsync(Guid requestId, string userId, string reason);

        Task<QuoteRequestResponse> CancelByUserAsync(Guid requestId, string userId);
    }
}