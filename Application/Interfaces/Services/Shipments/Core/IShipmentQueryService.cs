using Application.DTOs.Shipments.Core;
using Application.Models;

namespace Application.Interfaces.Services.Shipments.Core
{
    public interface IShipmentQueryService
    {
        Task<ShipmentResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged);
        Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(ShipmentParameters parameters);
        Task<IReadOnlyList<ShipmentResponse>> GetAllForUserAsync(string userId, ShipmentParameters parameters);
        Task<int> CountAsync();
    }
}