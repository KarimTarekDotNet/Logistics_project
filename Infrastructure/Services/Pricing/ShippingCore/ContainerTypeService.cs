using Application.DTOs.Aliases;
using Application.DTOs.ShippingCore;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Aliases;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using AutoMapper;
using Domain.Entities.ShippingCore;
using Domain.Enums;
using Domain.Exceptions;

namespace Infrastructure.Services.Pricing.ShippingCore
{
    public class ContainerTypeService : IContainerTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasService _aliasService;
        private readonly IMapper _mapper;

        public ContainerTypeService(IUnitOfWork unitOfWork, IMapper mapper, IAliasService aliasService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aliasService = aliasService;
        }

        public async Task<ContainerTypeResponse?> GetByIdAsync(Guid id)
        {
            var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(id);
            if (containerType == null || containerType.IsDeleted)
                return null;

            return _mapper.Map<ContainerTypeResponse>(containerType);
        }

        public async Task<IEnumerable<ContainerTypeResponse>> GetAllAsync(QueryParameters query)
        {
            var containerTypes = await _unitOfWork.ContainerTypes.GetAllAsync(query);
            return _mapper.Map<IEnumerable<ContainerTypeResponse>>(containerTypes);
        }

        public async Task<ContainerTypeResponse> CreateAsync(CreateContainerTypeRequest dto)
        {
            var existing = await _unitOfWork.ContainerTypes.GetByNameAsync(dto.Name);

            if (existing != null && !existing.IsDeleted)
                throw new BusinessRuleException($"A container type with name '{dto.Name}' already exists.");

            var containerType = _mapper.Map<ContainerType>(dto);
            containerType.CreatedAt = DateTimeOffset.UtcNow;
            containerType.IsDeleted = false;

            await _unitOfWork.ContainerTypes.AddAsync(containerType);
            await _unitOfWork.SaveChangesAsync();

            await _aliasService.CreateAsync(new CreateAliasRequest
            {
                AliasName = containerType.Name,
                EntityId = containerType.Id,
                Type = AliasType.ContainerType
            });

            return _mapper.Map<ContainerTypeResponse>(containerType);
        }

        public async Task<ContainerTypeResponse> UpdateAsync(Guid id, UpdateContainerTypeRequest dto)
        {
            var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(id);
            if (containerType == null || containerType.IsDeleted)
                throw new KeyNotFoundException("Container type not found.");

            var existing = await _unitOfWork.ContainerTypes.GetAllAsync(ct =>
                !ct.IsDeleted && ct.Name == dto.Name && ct.Id != id);

            if (existing.Any())
                throw new BusinessRuleException($"A container type with name '{dto.Name}' already exists.");

            containerType.Name = dto.Name;
            containerType.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();


            await _aliasService.CreateAsync(new CreateAliasRequest
            {
                AliasName = containerType.Name,
                EntityId = containerType.Id,
                Type = AliasType.ContainerType
            });

            return _mapper.Map<ContainerTypeResponse>(containerType);
        }

        public async Task DeleteAsync(Guid id)
        {
            var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(id);
            if (containerType == null || containerType.IsDeleted)
                throw new KeyNotFoundException("Container type not found.");

            containerType.IsDeleted = true;
            containerType.DeletedAt = DateTimeOffset.UtcNow;
            containerType.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}