using Application.DTOs.Shipments.Core;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentService
    {
        Task<ShipmentResponse?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(ShipmentParameters parameters);

        Task<ShipmentResponse> CreateAsync(CreateShipmentRequest request);
        Task<ShipmentResponse?> UpdateAsync(Guid id, UpdateShipmentRequest request);
        Task<bool> DeleteAsync(Guid id);

        Task<ShipmentResponse?> ChangeStatusAsync(Guid id, ChangeShipmentStatusRequest request);
    }

    public interface IShipmentStatusHistoryService
    {
        Task<IReadOnlyList<ShipmentStatusHistoryResponse>> GetByShipmentIdAsync(Guid shipmentId, QueryParameters parameters);
    }
}
