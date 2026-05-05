using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentChargeService
    {
        Task<ShipmentChargeResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<IReadOnlyList<ShipmentChargeResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged);

        Task<ShipmentChargeResponse> CreateAsync(CreateShipmentChargeRequest request);
        Task<ShipmentChargeResponse?> UpdateAsync(Guid id, UpdateShipmentChargeRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
