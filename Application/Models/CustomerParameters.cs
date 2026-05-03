namespace Application.Models
{
    public class CustomerParameters : QueryParameters
    {
        public DateOnly? DateOfBirth { get; set; }

        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }

        public DateTimeOffset? DeletedFrom { get; set; }
        public DateTimeOffset? DeletedTo { get; set; }
    }
    public class ShipmentParameters : QueryParameters
    {
        public DateTimeOffset? CreatedFrom { get; set; }
        public DateTimeOffset? CreatedTo { get; set; }

        public DateTimeOffset? DeliveredFrom { get; set; }
        public DateTimeOffset? DeliveredTo { get; set; }


    }
}