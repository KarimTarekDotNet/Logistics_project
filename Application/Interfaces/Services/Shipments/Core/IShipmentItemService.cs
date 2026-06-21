using Application.Common;
using Application.DTOs.Shipments.Core;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentItemService
    {
        Task<Result<ShipmentItemResponse>> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<Result<IReadOnlyList<ShipmentItemResponse>>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged);
        Task<Result<ShipmentItemResponse>> CreateAsync(CreateShipmentItemRequest request, string userId, bool isPrivileged);
        Task<Result<ShipmentItemResponse>> UpdateAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentItemRequest request);
        Task<Result<bool>> DeleteAsync(Guid id, string userId, bool isPrivileged);
    }
}
