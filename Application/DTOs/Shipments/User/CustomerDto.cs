using Application.DTOs.Pricing.Quotation;
using Application.DTOs.Shipments.Core;

namespace Application.DTOs.Shipments.User
{
    public record CreateCustomerRequest
    {
        public string NationalId { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? CountryCode { get; set; }
    }
    public record UpdateCustomerRequest
    {
        public string? NationalId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? CountryCode { get; set; }
    }
    public record CustomerResponse
    {
        public Guid Id { get; set; }
        public string ApplicationUserId { get; set; } = null!;
        public string? NationalId { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public ICollection<ShipmentResponse> Shipments { get; set; } = new List<ShipmentResponse>();
        public ICollection<QuoteResponse> Quotes { get; set; } = new List<QuoteResponse>();
    }
}