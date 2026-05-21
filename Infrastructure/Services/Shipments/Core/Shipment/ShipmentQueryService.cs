using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using Application.Models;
using AutoMapper;
using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Shipments.Core.Shipment
{
    public class ShipmentQueryService : IShipmentQueryService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ShipmentQueryService(UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<ShipmentResponse>> GetAllAsync(ShipmentParameters parameters)
        {
            var shipments = await _unitOfWork.Shipments.GetAllAsync(parameters);
            if (shipments == null || shipments.Count == 0)
                return new List<ShipmentResponse>();
            return _mapper.Map<IReadOnlyList<ShipmentResponse>>(shipments);
        }

        public async Task<int> CountAsync()
        {
            var count = await _unitOfWork.Shipments.CountAsync();
            if (count >= 0)
                return count.Value;

            else
                return 0;
        }

        public async Task<IReadOnlyList<ShipmentResponse>> GetAllForUserAsync(string userId, ShipmentParameters parameters)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null || user.CustomerProfile == null)
                return new List<ShipmentResponse>();

            var shipments = await _unitOfWork.Shipments.GetAllForUserAsync(user.CustomerProfile.Id, parameters);
            if (shipments == null || shipments.Count == 0)
                return new List<ShipmentResponse>();

            return _mapper.Map<IReadOnlyList<ShipmentResponse>>(shipments);
        }

        public async Task<ShipmentResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                return null;

            var shipment = await _unitOfWork.Shipments.GetByIdWithDetailsAsync(id);
            if (shipment == null)
                return null;

            if (isPrivileged)
                return _mapper.Map<ShipmentResponse>(shipment);

            if (user.CustomerProfile == null)
                return null;

            if (shipment.CustomerId != user.CustomerProfile.Id)
                return null;

            return _mapper.Map<ShipmentResponse>(shipment);
        }
    }
}
