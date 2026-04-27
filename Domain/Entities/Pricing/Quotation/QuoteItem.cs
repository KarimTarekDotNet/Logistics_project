namespace Domain.Entities.Pricing.Quotation
{
    public class QuoteItem // Individual item or service included in a shipping quote, such as additional fees or services
    {
        public Guid Id { get; set; }

        public Guid QuoteId { get; set; }
        public Quote Quote { get; set; } = null!;

        public string Description { get; set; } = null!;
        public decimal Amount { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset? DeletedAt { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
    }
}
