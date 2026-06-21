using Application.Common;
using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IInvoiceService
    {
        Task<Result<InvoiceResponse>> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<Result<IReadOnlyList<InvoiceResponse>>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged);
        Task<Result<InvoiceResponse>> CreateOrUpdateDraftInvoiceAsync(Guid shipmentId, string userId);
        Task<Result<InvoiceResponse>> ConfirmAsync(Guid id, string userId);
        Task<Result<InvoiceResponse>> CancelAsync(Guid id, string userId, bool isPrivileged, string reason);
        Task<Result<bool>> DeleteAsync(Guid id, string userId);
    }
}
