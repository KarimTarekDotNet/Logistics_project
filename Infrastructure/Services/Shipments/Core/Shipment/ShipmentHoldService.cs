using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork,
                    id, userId, isPrivileged, ShipmentStatus.OnHold, request.Reason);

                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }

                var oldStatus = shipment.StatusHistory
                    .OrderByDescending(h => h.ChangedAt)
                    .Skip(1)
                    .FirstOrDefault()?.ToStatus ?? shipment.Status;

                shipment.HoldReason = request.Reason;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(PutOnHoldAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
                    NewValues = JsonSerializer.Serialize(new { Status = ShipmentStatus.OnHold.ToString(), Reason = request.Reason }),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<ShipmentResponse>(shipment);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ShipmentResponse?> ResumeFromHoldAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null)
                    throw new BusinessRuleException("User not found.");

                if (!isPrivileged)
                    throw new BusinessRuleException("User does not have permission to resume shipment.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);

                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }

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

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(ResumeFromHoldAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(new { Status = ShipmentStatus.OnHold.ToString() }),
                    NewValues = JsonSerializer.Serialize(new { Status = previousStatus.ToString() }),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<ShipmentResponse>(shipment);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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
