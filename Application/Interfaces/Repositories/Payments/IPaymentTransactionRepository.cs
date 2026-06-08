using Application.Models;
using Domain.Entities.Payments;

namespace Application.Interfaces.Repositories.Payments
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction?> GetByIdAsync(Guid id);
        Task<List<PaymentTransaction>?> GetByInvoiceIdAsync(Guid invoiceId);
        Task<PaymentTransaction?> GetByProviderOrderIdAsync(string providerOrderId);
        Task<PaymentTransaction?> GetByIdToCurrentUserAsync(Guid id, string userId);
        Task AddAsync(PaymentTransaction transaction);
        void RemoveRange(IEnumerable<PaymentTransaction> transactions);
        void Remove(PaymentTransaction transaction);
        Task<List<PaymentTransaction>> GetAllAsync(QueryParameters query);
        Task<PaymentTransaction?> GetBySubscriptionPlanIdAsync(Guid subscriptionPlanId, string userId);
    }
}
