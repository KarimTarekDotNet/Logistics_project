using Application.Common;
using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentCommandService
    {
        Task<Result<ShipmentResponse>> CreateAsync(string userId, CreateShipmentRequest request);
        Task<Result<ShipmentResponse>> UpdateAsync(Guid id, UpdateShipmentRequest request, string userId);
        Task<Result<bool>> DeleteAsync(Guid id, string userId);
    }
}
