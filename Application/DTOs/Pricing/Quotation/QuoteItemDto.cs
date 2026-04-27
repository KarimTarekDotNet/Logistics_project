namespace Application.DTOs.Pricing.Quotation
{
    public record QuoteItemResponse(Guid Id, Guid QuoteId, string Description, decimal Amount);

    public record CreateQuoteItemRequest(string Description, decimal Amount);

    public record UpdateQuoteItemRequest(string Description, decimal Amount);
}
