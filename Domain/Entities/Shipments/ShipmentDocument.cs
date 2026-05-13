using Domain.Enums;

namespace Domain.Entities.Shipments
{
    public class ShipmentDocument
    {
        public Guid Id { get; set; }

        public Guid ShipmentId { get; set; }
        public Shipment Shipment { get; set; } = null!;

        public DocumentType Type { get; set; }

        public string FileName { get; set; } = null!;
        public string StoragePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;

        public string UploadedByUserId { get; set; } = null!;
        public DateTimeOffset UploadedAt { get; set; }

        public Guid? IntegrationMessageId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }
}
