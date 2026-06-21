using Application.Common;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentCancellationService : IShipmentCancellationService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentCancellationService> _logger;

        public ShipmentCancellationService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ShipmentCancellationService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ShipmentResponse>> CancelAsync(Guid id, string userId, bool isPrivileged, ChangeShipmentStatusRequest request)
        {
            _logger.LogInformation("Cancelling shipment {Id} by user {UserId}", id, userId);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var shipment = await HelperMethods.ChangeStatusAsync(_userManager, _unitOfWork, id, userId, isPrivileged, ShipmentStatus.Cancelled, request.Reason);
                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<ShipmentResponse>.NotFound("Shipment not found.");
                }

                var oldStatus = shipment.StatusHistory.OrderByDescending(h => h.ChangedAt).Skip(1).FirstOrDefault()?.ToStatus ?? shipment.Status;

                shipment.DraftBlApprovedAt = DateTime.UtcNow;
                shipment.CancellationReason = request.Reason;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(), Action = nameof(CancelAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(new { Status = oldStatus.ToString() }),
                    NewValues = JsonSerializer.Serialize(new { Status = ShipmentStatus.Cancelled.ToString(), Reason = request.Reason }),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Shipment {Id} cancelled", id);
                return Result<ShipmentResponse>.Success(_mapper.Map<ShipmentResponse>(shipment));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Cancellation failed for shipment {Id}", id);
                throw;
            }
        }
    }
}
