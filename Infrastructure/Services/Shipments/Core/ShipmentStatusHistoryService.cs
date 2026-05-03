using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using AutoMapper;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentStatusHistoryService : IShipmentStatusHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentStatusHistoryService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ShipmentStatusHistoryResponse>> GetByShipmentIdAsync(Guid shipmentId, QueryParameters parameters)
        {
            var statusHistory = await _unitOfWork.StatusHistoryRepositories.GetByShipmentIdAsync(shipmentId, parameters);
            if(!statusHistory.Any())
                return new List<ShipmentStatusHistoryResponse>();

            return _mapper.Map<IReadOnlyList<ShipmentStatusHistoryResponse>>(statusHistory);
        }
    }
}
