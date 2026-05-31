using Domain.Entities.Shipments;

namespace Application.Interfaces.Repositories.Shipments.Core
{
    public interface IInvoicePaymentRepository
    {
        Task<InvoicePayment?> GetByIdAsync(Guid id);

        Task<InvoicePayment?> GetByTransactionIdAsync(string transactionId);

        Task<InvoicePayment?> GetByReferenceNumberAsync(string referenceNumber);

        Task<IReadOnlyList<InvoicePayment>> GetByInvoiceIdAsync(Guid invoiceId, Guid customerId);
        Task<IReadOnlyList<InvoicePayment>> GetByInvoiceIdAsync(Guid invoiceId);

        Task AddAsync(InvoicePayment payment);
    }
}
