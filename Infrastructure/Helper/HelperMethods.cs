using Application.ApplicationRules.Shipments;
using Application.Interfaces.Repositories.Patterns;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Helper
{
    public static class HelperMethods
    {
        public static async Task<Shipment?> ChangeStatusAsync( UserManager<ApplicationUser> _userManager,
            IUnitOfWork _unitOfWork,
            Guid id, string userId, bool isPrivileged, ShipmentStatus targetStatus, string? reason)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new BusinessRuleException("User not found.");

            if (!isPrivileged)
                throw new BusinessRuleException("User does not have permission to change shipment status.");

            var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
            if (shipment == null)
                return null;

            var oldStatus = shipment.Status;

            if (!ShipmentStatusRules.CanTransition(oldStatus, targetStatus))
                throw new BusinessRuleException($"Cannot change shipment status from {oldStatus} to {targetStatus}.");

            var now = DateTimeOffset.UtcNow;

            shipment.Status = targetStatus;
            shipment.UpdatedAt = now;

            shipment.StatusHistory.Add(new ShipmentStatusHistory
            {
                ChangedByUserId = userId,
                ChangedByRole = _userManager.GetRolesAsync(user).Result.FirstOrDefault(),
                ChangedBy = user.UserName,
                ShipmentId = shipment.Id,
                FromStatus = oldStatus,
                ToStatus = targetStatus,
                ChangedAt = now,
                Reason = reason
            });

            return shipment;
        }
    }
}
