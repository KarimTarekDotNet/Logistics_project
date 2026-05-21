using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentItemService
    {
        Task<ShipmentItemResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<IReadOnlyList<ShipmentItemResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged);

        Task<ShipmentItemResponse> CreateAsync(CreateShipmentItemRequest request, string userId, bool isPrivileged);
        Task<ShipmentItemResponse?> UpdateAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentItemRequest request);
        Task<bool> DeleteAsync(Guid id, string userId, bool isPrivileged);
    }
}