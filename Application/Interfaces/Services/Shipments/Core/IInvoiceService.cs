using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IInvoiceService
    {
        Task<InvoiceResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<IReadOnlyList<InvoiceResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged);

        Task<InvoiceResponse> CreateOrUpdateDraftInvoiceAsync(Guid shipmentId, string userId);
        Task<InvoiceResponse?> ConfirmAsync(Guid id, string userId);
        Task<InvoiceResponse?> CancelAsync(Guid id, string userId, bool isPrivileged, string reason);

        Task<bool> DeleteAsync(Guid id, string userId);
    }
}