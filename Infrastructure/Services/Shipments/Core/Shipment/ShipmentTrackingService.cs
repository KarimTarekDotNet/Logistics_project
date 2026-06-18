using Application.ApplicationRules.Shipments;
using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

                if (user == null)
                    throw new BusinessRuleException("User not found.");

                if (!isPrivileged)
                    throw new BusinessRuleException("User does not have permission to update shipment tracking.");

                var shipment = await _unitOfWork.Shipments.GetTrackedByIdWithDetailsAsync(id);
                if (shipment == null)
                {
                    await _unitOfWork.RollbackTransactionAsync();
                    return null;
                }

                var oldShipment = shipment;

                ShipmentTrackingRules.ApplyTrackingUpdate(shipment, request);
                shipment.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = shipment.Id,
                    EntityName = nameof(Domain.Entities.Shipments.Shipment).ToUpper(),
                    Action = nameof(UpdateTrackingAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldShipment),
                    NewValues = JsonSerializer.Serialize(shipment),
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
    }
}
