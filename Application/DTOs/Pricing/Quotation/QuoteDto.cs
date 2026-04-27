namespace Application.DTOs.Pricing.Quotation
{
    public record QuoteResponse
    {
        public Guid Id { get; init; }
        public string CustomerName { get; init; } = null!;
        public Guid RouteId { get; init; }
        public string FromPortCode { get; init; } = null!;
        public string ToPortCode { get; init; } = null!;
        public Guid ContainerTypeId { get; init; }
        public string ContainerTypeName { get; init; } = null!;
        public decimal FinalPrice { get; init; }
        public string Currency { get; init; } = null!;
        public DateTimeOffset CreatedAt { get; init; }
        public IReadOnlyList<QuoteItemResponse> Items { get; init; } = null!;
    }

    public record CreateQuoteRequest(
        string CustomerName,
        Guid RouteId,
        Guid ContainerTypeId,
        decimal FinalPrice,
        string Currency,
        IReadOnlyList<CreateQuoteItemRequest> Items);

    public record UpdateQuoteRequest(
        string CustomerName,
        decimal FinalPrice,
        string Currency);
}