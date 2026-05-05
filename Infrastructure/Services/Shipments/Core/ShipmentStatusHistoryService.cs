using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using AutoMapper;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentStatusHistoryService : IShipmentStatusHistoryService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentStatusHistoryService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<IReadOnlyList<ShipmentStatusHistoryResponse>>GetByShipmentIdAsync(Guid shipmentId, string userId,
        bool isPrivileged, QueryParameters parameters)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);
            if (shipment == null)
                return new List<ShipmentStatusHistoryResponse>();

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || user.CustomerProfile.Id != shipment.CustomerId)
                    return new List<ShipmentStatusHistoryResponse>();
            }

            var statusHistory = await _unitOfWork.StatusHistoryRepositories.GetByShipmentIdAsync(shipmentId, parameters);

            return _mapper.Map<IReadOnlyList<ShipmentStatusHistoryResponse>>(statusHistory);
        }
    }
}
