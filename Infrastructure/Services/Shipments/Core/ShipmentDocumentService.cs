using Application.DTOs.Shipments.Core;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Shipments.Core;
using AutoMapper;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Shipments.Core
{
    public class ShipmentDocumentService : IShipmentDocumentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IFileStorageService _fileStorageService;
        private readonly IFileSecurityService _fileSecurityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ShipmentDocumentService(IUnitOfWork unitOfWork, IMapper mapper,
        IFileStorageService fileStorageService, IFileSecurityService fileSecurityService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _fileStorageService = fileStorageService;
            _fileSecurityService = fileSecurityService;
            _userManager = userManager;
        }

        public async Task<ShipmentDocumentResponse> UploadAsync(Guid shipmentId, UploadShipmentDocumentRequest request, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var shipment = await _unitOfWork.Shipments.GetByIdAsync(shipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || user.CustomerProfile.Id != shipment.CustomerId)
                    throw new BusinessRuleException("You do not have permission to access this shipment.");
            }

            await _fileSecurityService.ValidateAsync(request.File);

            await _fileSecurityService.ScanAsync(request.File);

            var folder = Path.Combine("shipments", shipmentId.ToString(), "documents");

            var storagePath = await _fileStorageService.UploadAsync(request.File, folder);


            var newDocument = new ShipmentDocument
            {
                ShipmentId = shipment.Id,
                Type = request.Type,
                FileName = Path.GetFileName(request.File.FileName),
                StoragePath = storagePath,
                ContentType = request.File.ContentType,
                UploadedByUserId = userId,
                UploadedAt = DateTimeOffset.UtcNow,
                IntegrationMessageId = request.IntegrationMessageId
            };

            await _unitOfWork.ShipmentDocuments.AddAsync(newDocument);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<ShipmentDocumentResponse>(newDocument);
        }

        public async Task DeleteAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var shipmentDocument = await _unitOfWork.ShipmentDocuments.GetByIdAsync(id);
            if (shipmentDocument == null)
                throw new KeyNotFoundException("Shipment not found");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || user.CustomerProfile.Id != shipmentDocument.Shipment.CustomerId)
                    throw new BusinessRuleException("You do not have permission to access this shipment.");
            }
            _unitOfWork.ShipmentDocuments.Delete(shipmentDocument);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ShipmentDocumentResponse?> GetByIdAsync(Guid id, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var shipmentDocument = await _unitOfWork.ShipmentDocuments.GetByIdAsync(id);
            if (shipmentDocument == null)
                throw new KeyNotFoundException("Shipment not found");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || user.CustomerProfile.Id != shipmentDocument.Shipment.CustomerId)
                    throw new BusinessRuleException("You do not have permission to access this shipment.");
            }

            return _mapper.Map<ShipmentDocumentResponse>(shipmentDocument);
        }

        public async Task<IReadOnlyCollection<ShipmentDocumentResponse>> GetByShipmentIdAsync(Guid shipmentId, string userId, bool isPrivileged)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
                throw new UnauthorizedAccessException("User not found");

            var shipment = await _unitOfWork.ShipmentDocuments.GetByShipmentIdAsync(shipmentId);
            if (shipment == null)
                throw new KeyNotFoundException("Shipment not found");

            if (!isPrivileged)
            {
                if (user.CustomerProfile == null || shipment.Any(x => x.Shipment.CustomerId != user.CustomerProfile.Id))
                    throw new BusinessRuleException("You do not have permission to access this shipment.");
            }
            return _mapper.Map<IReadOnlyCollection<ShipmentDocumentResponse>>(shipment);

        }
    }
}
