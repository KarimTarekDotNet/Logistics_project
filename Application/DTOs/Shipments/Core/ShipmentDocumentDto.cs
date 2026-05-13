using Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs.Shipments.Core
{
    public record ShipmentDocumentResponse
    {
        public Guid Id { get; set; }
        public Guid ShipmentId { get; set; }

        public string Type { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string StoragePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;

        public string UploadedByUserId { get; set; } = null!;
        public string UploadedByUsername { get; set; } = null!;

        public DateTimeOffset UploadedAt { get; set; }

        public IntegrationMessageResponse? IntegrationMessage { get; set; }
    }

    public record UploadShipmentDocumentRequest
    {
        public DocumentType Type { get; set; }
        public IFormFile File { get; set; } = null!;
        public Guid? IntegrationMessageId { get; set; }
    }

    public record IntegrationMessageResponse
    {
        public Guid Id { get; set; }
        public string ExternalMessageId { get; set; } = null!;
        public string Source { get; set; } = null!;
        public string ProcessingStatus { get; set; } = null!;
        public DateTimeOffset CreatedAt { get; set; }
    }
}
