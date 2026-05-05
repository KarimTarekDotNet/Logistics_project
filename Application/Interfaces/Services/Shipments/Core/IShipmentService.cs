using Application.DTOs.Shipments.Core;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentService
    {
        Task<ShipmentResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(ShipmentParameters parameters);
        Task<IReadOnlyList<ShipmentResponse>> GetAllForUserAsync(string userId, ShipmentParameters parameters);

        Task<ShipmentResponse> CreateAsync(string userId, CreateShipmentRequest request);
        Task<ShipmentResponse?> UpdateAsync(Guid id, string userId, UpdateShipmentRequest request);
        Task<bool> DeleteAsync(Guid id, string userId);

        Task<ShipmentResponse?> ChangeStatusAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request);
    }

    public interface IShipmentStatusHistoryService
    {
        Task<IReadOnlyList<ShipmentStatusHistoryResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged, QueryParameters parameters);
    }
}
