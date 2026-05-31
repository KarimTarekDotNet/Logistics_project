using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IInvoicePaymentService
    {
        Task<IReadOnlyList<InvoicePaymentResponse>> GetPaymentsByInvoiceIdAsync(Guid invoiceId, string userId, bool isPrivileged);
        Task<InvoiceResponse?> MarkAsPaidAsync(Guid id, CreateInvoicePaymentRequest request);
        Task<InvoiceResponse?> MarkAsPartiallyPaidAsync(Guid id, CreateInvoicePaymentRequest request);
        Task<InvoiceResponse?> MarkAsRefundedAsync(Guid id);
    }
}