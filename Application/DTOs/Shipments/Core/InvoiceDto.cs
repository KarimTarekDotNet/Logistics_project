using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Shipments.Core
{
    public record CreateInvoiceRequest
    {
        public Guid ShipmentId { get; set; }
        public List<Guid> ShipmentChargeIds { get; set; } = [];

        public string Currency { get; set; } = "USD";
        public PayerType PayerType { get; set; }
        public DateTimeOffset DueDate { get; set; }
    }
    public record InvoiceResponse
    {
        public Guid Id { get; set; }

        public ShipmentResponse Shipment { get; set; } = null!;

        public string InvoiceNumber { get; set; } = null!;
        public string Currency { get; set; } = null!;

        public ICollection<ShipmentChargeResponse> Charges { get; set; } = new List<ShipmentChargeResponse>();

        public decimal SubTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string PaymentStatus { get; set; } = null!;

        public DateTimeOffset IssuedAt { get; set; }
        public DateTimeOffset DueDate { get; set; }
        public DateTimeOffset? PaidAt { get; set; }

        public string PayerType { get; set; } = null!;
    }
    public record CancelInvoiceRequest
    {
        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;
    }
}