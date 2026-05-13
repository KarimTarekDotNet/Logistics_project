using Application.ApplicationRules.Shipments;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentTrackingService : IShipmentTrackingService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentTrackingService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentResponse?> UpdateTrackingAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentTrackingRequest request)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new BusinessRuleException("User not found.");

            if (!isPrivileged)
                throw new BusinessRuleException("User does not have permission to update shipment tracking.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
            if (shipment == null)
                return null;

            ShipmentTrackingRules.ApplyTrackingUpdate(shipment, request);

            shipment.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentResponse>(shipment);
        }
    }
}
