using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;

namespace Domain.Entities.Users
{
    public class Customer
    {
        public Guid Id { get; set; }

        public string ApplicationUserId { get; set; } = null!;
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public string? NationalId { get; set; }
        public DateOnly? DateOfBirth { get; set; }

        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? CountryCode { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
        public ICollection<Quote> Quotes { get; set; } = new List<Quote>();
    }
}