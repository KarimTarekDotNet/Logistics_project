using System.Text.Json.Serialization;

namespace Application.DTOs.Payment
{
    public record CreatePaymobIntentionRequest
    {
        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";

        [JsonPropertyName("payment_methods")]
        public List<int> PaymentMethods { get; set; } = new();

        [JsonPropertyName("items")]
        public List<PaymobItemRequest> Items { get; set; } = new();


        [JsonPropertyName("billing_data")]
        public PaymobBillingDataRequest BillingData { get; set; } = null!;


        [JsonPropertyName("special_reference")]
        public string SpecialReference { get; set; } = null!;


        [JsonPropertyName("notification_url")]
        public string? NotificationUrl { get; set; }


        [JsonPropertyName("redirection_url")]
        public string? RedirectionUrl { get; set; }
    }

    public record PaymobItemRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("amount")]
        public int Amount { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;
    }

    public record PaymobBillingDataRequest
    {
        [JsonPropertyName("first_name")]
        public string FirstName { get; set; } = null!;

        [JsonPropertyName("last_name")]
        public string LastName { get; set; } = null!;

        [JsonPropertyName("phone_number")]
        public string? PhoneNumber { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("country")]
        public string Country { get; set; } = "EG";

        [JsonPropertyName("email")]
        public string Email { get; set; } = null!;

        [JsonPropertyName("apartment")]
        public string? Apartment { get; set; }

        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("building")]
        public string? Building { get; set; }

        [JsonPropertyName("floor")]
        public string? Floor { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

    public record CreatePaymobIntentionResponse
    {
        [JsonPropertyName("id")]
        public string IntentionId { get; set; } = null!;

        [JsonPropertyName("intention_order_id")]
        public long OrderId { get; set; }

        [JsonPropertyName("client_secret")]
        public string ClientSecret { get; set; } = null!;

        [JsonPropertyName("special_reference")]
        public string? SpecialReference { get; set; }
    }

    public record PaymobTransactionWebhookRequest
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("obj")]
        public PaymobTransactionWebhookObject Obj { get; set; } = null!;
    }

    public record PaymobTransactionWebhookObject
    {
        [JsonPropertyName("id")]
        public long TransactionId { get; set; }

        [JsonPropertyName("pending")]
        public bool Pending { get; set; }

        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("amount_cents")]
        public int AmountCents { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = null!;

        [JsonPropertyName("error_occured")]
        public bool ErrorOccured { get; set; }

        [JsonPropertyName("order")]
        public PaymobWebhookOrder Order { get; set; } = null!;

        [JsonPropertyName("created_at")]
        public string CreatedAt { get; set; } = null!;

        [JsonPropertyName("has_parent_transaction")]
        public bool HasParentTransaction { get; set; }

        [JsonPropertyName("integration_id")]
        public long IntegrationId { get; set; }

        [JsonPropertyName("is_3d_secure")]
        public bool Is3DSecure { get; set; }

        [JsonPropertyName("is_auth")]
        public bool IsAuth { get; set; }

        [JsonPropertyName("is_capture")]
        public bool IsCapture { get; set; }

        [JsonPropertyName("is_refunded")]
        public bool IsRefunded { get; set; }

        [JsonPropertyName("is_standalone_payment")]
        public bool IsStandalonePayment { get; set; }

        [JsonPropertyName("is_voided")]
        public bool IsVoided { get; set; }

        [JsonPropertyName("owner")]
        public long Owner { get; set; }

        [JsonPropertyName("source_data")]
        public PaymobCallbackSourceData SourceData { get; set; } = null!;
    }

    public record PaymobCallbackSourceData
    {
        [JsonPropertyName("pan")]
        public string? Pan { get; set; }

        [JsonPropertyName("sub_type")]
        public string? SubType { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
    }

    public record PaymobWebhookOrder
    {
        [JsonPropertyName("id")]
        public long OrderId { get; set; }
    }

    public record CheckoutPaymentResponse
    {
        public string CheckoutUrl { get; set; } = null!;
    }
}   