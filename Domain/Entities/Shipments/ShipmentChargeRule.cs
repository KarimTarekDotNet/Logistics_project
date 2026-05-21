using Domain.Enums;

namespace Domain.Entities.Shipments
{
    public class ShipmentChargeRule
    {
        public Guid Id { get; set; }
        public ChargeType ChargeType { get; set; }
        public PayerType PayerType { get; set; }
        public ChargeCalculationType CalculationType { get; set; }
        public decimal Value { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsActive { get; set; } = true;
    }
}