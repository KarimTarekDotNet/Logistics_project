using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentHoldService : IShipmentHoldService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentHoldService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ShipmentResponse?> PutOnHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var shipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, 
                id, userId, isPrivileged, ShipmentStatus.OnHold, request.Reason);

            if (shipment == null)
                return null;

            shipment.HoldReason = request.Reason;
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentResponse>(shipment);
        }

        public async Task<ShipmentResponse?> ResumeFromHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new BusinessRuleException("User not found.");

            if (!isPrivileged)
                throw new BusinessRuleException("User does not have permission to resume shipment.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);

            if (shipment == null)
                return null;

            if (shipment.Status != ShipmentStatus.OnHold)
                throw new BusinessRuleException("Shipment is not currently on hold.");

            var previousStatus = GetPreviousStatusBeforeHold(shipment);

            var now = DateTimeOffset.UtcNow;

            shipment.Status = previousStatus;
            shipment.UpdatedAt = now;

            shipment.StatusHistory.Add(new ShipmentStatusHistory
            {
                ChangedByUserId = userId,
                ChangedByRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault(),
                ChangedBy = user.UserName,
                ShipmentId = shipment.Id,
                FromStatus = ShipmentStatus.OnHold,
                ToStatus = previousStatus,
                ChangedAt = now,
                Reason = request.Reason
            });

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentResponse>(shipment);
        }

        private ShipmentStatus GetPreviousStatusBeforeHold(Domain.Entities.Shipments.Shipment shipment)
        {
            var holdEntry = shipment.StatusHistory
                .Where(h => h.ToStatus == ShipmentStatus.OnHold)
                .OrderByDescending(h => h.ChangedAt)
                .FirstOrDefault();

            if (holdEntry == null)
                throw new BusinessRuleException("Cannot resume shipment because hold history was not found.");

            return holdEntry.FromStatus;
        }
    }
}
