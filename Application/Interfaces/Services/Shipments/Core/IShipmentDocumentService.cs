using Application.DTOs.Shipments.Core;
using Microsoft.AspNetCore.Http;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentDocumentService
    {
        Task<ShipmentDocumentResponse> UploadAsync(Guid shipmentId, UploadShipmentDocumentRequest request, string userId, bool isPrivileged);
        Task<IReadOnlyCollection<ShipmentDocumentResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged);
        Task<ShipmentDocumentResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task DeleteAsync(Guid id, string userId, bool isPrivileged);
    }

    public interface IFileStorageService
    {
        Task<string> UploadAsync(IFormFile file, string folder);

        Task DeleteAsync(string path);
    }
    public interface IFileSecurityService
    {
        Task ValidateAsync(IFormFile file);
        Task ScanAsync(IFormFile file);
    }
}