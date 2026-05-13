using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentCancellationService : IShipmentCancellationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentCancellationService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentResponse?> CancelAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var newShipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork,
            id, userId, isPrivileged, ShipmentStatus.Cancelled, request.Reason);
            if (newShipment == null)
                return null;

            newShipment.DraftBlApprovedAt = DateTime.UtcNow;
            newShipment.CancellationReason = request.Reason;

            await _unitOfWork.SaveChangesAsync();
            return _mapper.Map<ShipmentResponse?>(newShipment);
        }
    }
}
