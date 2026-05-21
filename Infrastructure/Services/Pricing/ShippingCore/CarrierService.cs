using Application.ApplicationRules;
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
    public class CarrierService : ICarrierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasService _aliasService;
        private readonly IMapper _mapper;

        public CarrierService(IUnitOfWork unitOfWork, IMapper mapper, IAliasService aliasService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aliasService = aliasService;
        }

        public async Task<CarrierResponse?> GetByIdAsync(Guid id)
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null || carrier.IsDeleted)
                return null;

            return _mapper.Map<CarrierResponse>(carrier);
        }

        public async Task<CarrierResponse?> GetByNameOrCodeAsync(string input)
        {
            var carrier = await _unitOfWork.Carriers.GetByNameOrCodeAsync(input);

            if (carrier == null)
            {
                var resolved = await _aliasService.ResolveAsync(input, AliasType.Carrier);

                if (resolved is { Resolved: true, EntityId: not null })
                    carrier = await _unitOfWork.Carriers.GetByIdAsync(resolved.EntityId.Value);
            }

            return _mapper.Map<CarrierResponse>(carrier);
        }

        public async Task<IEnumerable<CarrierResponse>> GetAllAsync(QueryParameters query)
        {
            var carriers = await _unitOfWork.Carriers.GetAllAsync(query);
            return _mapper.Map<IEnumerable<CarrierResponse>>(carriers);
        }

        public async Task<CarrierResponse> CreateAsync(CreateCarrierRequest dto)
        {
            var existing = await _unitOfWork.Carriers.GetByNameOrCodeAsync(dto.Code);
            if (existing != null && !existing.IsDeleted)
                throw new BusinessRuleException($"A carrier with code '{dto.Code}' already exists.");

            var carrier = _mapper.Map<Carrier>(dto);
            carrier.CreatedAt = DateTimeOffset.UtcNow;
            carrier.IsDeleted = false;

            await _unitOfWork.Carriers.AddAsync(carrier);
            await _unitOfWork.SaveChangesAsync();

            await _aliasService.CreateAsync(new CreateAliasRequest
            {
                AliasName = carrier.Name,
                EntityId = carrier.Id,
                Type = AliasType.Carrier
            });

            await _aliasService.CreateAsync(new CreateAliasRequest
            {
                AliasName = carrier.Code,
                EntityId = carrier.Id,
                Type = AliasType.Carrier
            });


            return _mapper.Map<CarrierResponse>(carrier);
        }

        public async Task<CarrierResponse> UpdateAsync(Guid id, UpdateCarrierRequest dto)
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null || carrier.IsDeleted)
                throw new KeyNotFoundException("Carrier not found.");

            if(string.IsNullOrWhiteSpace(dto.Code))
                dto.Code = carrier.Code;

            if (!CarrierRule.IsCodeMatch(dto.Code))
                throw new BusinessRuleException("Carrier code must be exactly 4 uppercase letters (SCAC format).");

            var existing = await _unitOfWork.Carriers.GetByNameOrCodeAsync(dto.Code);
            if (existing != null && !existing.IsDeleted && existing.Id != id)
                throw new BusinessRuleException($"A carrier with code '{dto.Code}' already exists.");

            if(string.IsNullOrEmpty(dto.Name))
                dto.Name = carrier.Name;

            carrier.Name = dto.Name;
            carrier.Code = dto.Code;
            carrier.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            await _aliasService.CreateAsync(new CreateAliasRequest
            {
                AliasName = carrier.Name,
                EntityId = carrier.Id,
                Type = AliasType.Carrier
            });

            await _aliasService.CreateAsync(new CreateAliasRequest
            {
                AliasName = carrier.Code,
                EntityId = carrier.Id,
                Type = AliasType.Carrier
            });

            return _mapper.Map<CarrierResponse>(carrier);
        }

        public async Task DeleteAsync(Guid id)
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null || carrier.IsDeleted)
                throw new KeyNotFoundException("Carrier not found.");

            carrier.IsDeleted = true;
            carrier.DeletedAt = DateTimeOffset.UtcNow;
            carrier.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}