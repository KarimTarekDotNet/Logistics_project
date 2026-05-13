using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentCommandService
    {
        Task<ShipmentResponse> CreateAsync(string userId, CreateShipmentRequest request);
        Task<ShipmentResponse?> UpdateAsync(Guid id, UpdateShipmentRequest request);
        Task<bool> DeleteAsync(Guid id, string userId);
    }
}
