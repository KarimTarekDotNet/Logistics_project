using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentChargeService
    {
        Task<ShipmentChargeResponse?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ShipmentChargeResponse>> GetByShipmentIdAsync(Guid shipmentId);

        Task<ShipmentChargeResponse> CreateAsync(CreateShipmentChargeRequest request);
        Task<ShipmentChargeResponse?> UpdateAsync(Guid id, UpdateShipmentChargeRequest request);
        Task<bool> DeleteAsync(Guid id);
    }
}
