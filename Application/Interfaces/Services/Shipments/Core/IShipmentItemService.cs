using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentItemService
    {
        Task<ShipmentItemResponse?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ShipmentItemResponse>> GetByShipmentIdAsync(Guid shipmentId);

        Task<ShipmentItemResponse> CreateAsync(CreateShipmentItemRequest request);
        Task<ShipmentItemResponse?> UpdateAsync(Guid id, UpdateShipmentItemRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
