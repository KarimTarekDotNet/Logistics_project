using Application.DTOs.Payment;

namespace Application.Interfaces.Services.Payment
{
    public interface IPaymentTransactionService
    {
        Task<StartPaymentResponse> StartPaymentAsync(StartPaymentRequest request, string userId);

        Task<PaymentTransactionResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);

        Task HandlePaymobWebhookAsync(PaymobTransactionWebhookRequest request, string receivedHmac);

        Task<CheckoutPaymentResponse> CheckoutAsync(Guid paymentTransactionId, string userId);

        Task CancelPendingPaymentAsync(Guid paymentTransactionId, string userId);
    }

    public interface IPaymobPaymentService
    {
        Task<CreatePaymobIntentionResponse> CreateIntentionAsync(CreatePaymobIntentionRequest request);
    }
}
