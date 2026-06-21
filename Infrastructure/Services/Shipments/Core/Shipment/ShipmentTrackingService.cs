using Application.ApplicationRules.Shipments;
using Application.Common;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentTrackingService : IShipmentTrackingService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ShipmentTrackingService> _logger;

        public ShipmentTrackingService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper, ILogger<ShipmentTrackingService> logger)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ShipmentResponse>> UpdateTrackingAsync(Guid id, string userId, bool isPrivileged, UpdateShipmentTrackingRequest request)
        {
            _logger.LogInformation("Updating tracking for shipment {Id} by user {UserId}", id, userId);
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null)
                    return Result<ShipmentResponse>.NotFound("User not found.");

                if (!isPrivileged)
                    return Result<ShipmentResponse>.Forbidden("User does not have permission to update shipment tracking.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return Result<ShipmentResponse>.NotFound("Shipment not found.");
                }

                var oldShipment = shipment;
                ShipmentTrackingRules.ApplyTrackingUpdate(shipment, request);
                shipment.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(), Action = nameof(UpdateTrackingAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldShipment), NewValues = JsonSerializer.Serialize(shipment),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("Tracking updated for shipment {Id}", id);
                return Result<ShipmentResponse>.Success(_mapper.Map<ShipmentResponse>(shipment));
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "UpdateTracking failed for shipment {Id}", id);
                throw;
            }
        }
    }
}
