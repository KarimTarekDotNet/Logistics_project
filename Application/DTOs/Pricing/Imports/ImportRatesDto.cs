using Domain.Enums;

namespace Application.DTOs.Pricing.Imports
{
    public record ImportRatesRequest
    {
        public ExternalSource Source { get; set; } = ExternalSource.n8n;
        public List<ImportRateItemRequest> Rates { get; set; } = new();
    }

    public record ImportRateItemRequest
    {
        public string ExternalMessageId { get; set; } = default!;
        public string CarrierName { get; set; } = default!;
        public string FromPortCode { get; set; } = default!;
        public string ToPortCode { get; set; } = default!;
        public string ContainerTypeName { get; set; } = default!;
        public decimal Price { get; set; }
        public string Currency { get; set; } = default!;
        public DateTimeOffset ValidFrom { get; set; }
        public DateTimeOffset ValidTo { get; set; }
        public string? RawSubject { get; set; }
    }
    public record ImportRatesResponse
    {
        public int TotalReceived { get; set; }
        public int Imported { get; set; }
        public int Updated { get; set; }
        public int Skipped { get; set; }
        public int Failed { get; set; }
        public List<ImportRateItemResult> Results { get; set; } = new();
    }

    public record ImportRateItemResult
    {
        public string ExternalMessageId { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string? Message { get; set; }
        public Guid? RateId { get; set; }
    }
}
