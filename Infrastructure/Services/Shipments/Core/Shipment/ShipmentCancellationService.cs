using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using System.Text.Json;

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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork,
                id, userId, isPrivileged, ShipmentStatus.Cancelled, request.Reason);

                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }

                var oldStatus = shipment.StatusHistory
                    .OrderByDescending(h => h.ChangedAt)
                    .Skip(1)
                    .FirstOrDefault()?.ToStatus ?? shipment.Status;

                shipment.DraftBlApprovedAt = DateTime.UtcNow;
                shipment.CancellationReason = request.Reason;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(CancelAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
                    NewValues = JsonSerializer.Serialize(new { Status = ShipmentStatus.Cancelled.ToString(), Reason = request.Reason }),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                return _mapper.Map<ShipmentResponse?>(shipment);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}
