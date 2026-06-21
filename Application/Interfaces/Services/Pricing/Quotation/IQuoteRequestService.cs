using Application.Common;
using Application.DTOs.Pricing.Quotation;
using Application.Models;

namespace Application.Interfaces.Services.Pricing.Quotation
{
    public interface IQuoteRequestService
    {
        Task<Result<QuoteRequestResponse>> CreateFromRateAsync(CreateQuoteRequestFromRate request, string userId);
        Task<Result<IEnumerable<QuoteRequestResponse>>> GetMyRequestsAsync(string userId, QueryParameters query);
        Task<Result<QuoteRequestResponse>> GetByIdAsync(Guid id);
        Task<Result<IEnumerable<QuoteRequestResponse>>> GetAllAsync(string userId, QueryParameters query);
        Task<Result<QuoteRequestResponse>> ApproveAsync(Guid requestId, string userId);
        Task<Result<QuoteRequestResponse>> RejectAsync(Guid requestId, string userId, string reason);
        Task<Result<QuoteRequestResponse>> CancelByUserAsync(Guid requestId, string userId);
    }
}
