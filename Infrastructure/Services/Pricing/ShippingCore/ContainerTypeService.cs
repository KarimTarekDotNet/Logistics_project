using Application.Common;
using Application.DTOs.Aliases;
using Application.DTOs.ShippingCore;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Aliases;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.ShippingCore;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Pricing.ShippingCore
{
    public class ContainerTypeService : IContainerTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasService _aliasService;
        private readonly IMapper _mapper;
        private readonly ILogger<ContainerTypeService> _logger;

        public ContainerTypeService(IUnitOfWork unitOfWork, IMapper mapper, IAliasService aliasService, ILogger<ContainerTypeService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aliasService = aliasService;
            _logger = logger;
        }

        public async Task<Result<ContainerTypeResponse>> GetByIdAsync(Guid id)
        {
            var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(id);
            if (containerType == null || containerType.IsDeleted)
            {
                _logger.LogWarning("ContainerType {Id} not found", id);
                return Result<ContainerTypeResponse>.NotFound("Container type not found.");
            }
            return Result<ContainerTypeResponse>.Success(_mapper.Map<ContainerTypeResponse>(containerType));
        }

        public async Task<Result<IEnumerable<ContainerTypeResponse>>> GetAllAsync(QueryParameters query)
        {
            var containerTypes = await _unitOfWork.ContainerTypes.GetAllAsync(query);
            return Result<IEnumerable<ContainerTypeResponse>>.Success(_mapper.Map<IEnumerable<ContainerTypeResponse>>(containerTypes));
        }

        public async Task<Result<ContainerTypeResponse>> CreateAsync(CreateContainerTypeRequest dto, string userId)
        {
            _logger.LogInformation("Creating container type {Name} by user {UserId}", dto.Name, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var existing = await _unitOfWork.ContainerTypes.GetByNameAsync(dto.Name);
                if (existing != null && !existing.IsDeleted)
                {
                    _logger.LogWarning("Container type name {Name} already exists", dto.Name);
                    return Result<ContainerTypeResponse>.Failure($"A container type with name '{dto.Name}' already exists.");
                }

                var containerType = _mapper.Map<ContainerType>(dto);
                containerType.CreatedAt = DateTimeOffset.UtcNow;
                containerType.IsDeleted = false;

                var audit = new AuditLog
                {
                    CreatedAt = containerType.CreatedAt, EntityId = containerType.Id,
                    EntityName = nameof(ContainerType).ToUpper(), Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null, NewValues = JsonSerializer.Serialize(containerType), UserId = userId
                };

                await _unitOfWork.ContainerTypes.AddAsync(containerType);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = containerType.Name, EntityId = containerType.Id, Type = AliasType.ContainerType });

                _logger.LogInformation("ContainerType {Id} created successfully", containerType.Id);
                return Result<ContainerTypeResponse>.Success(_mapper.Map<ContainerTypeResponse>(containerType), 201);
            });
        }

        public async Task<Result<ContainerTypeResponse>> UpdateAsync(Guid id, UpdateContainerTypeRequest dto, string userId)
        {
            _logger.LogInformation("Updating container type {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(id);
                if (containerType == null || containerType.IsDeleted)
                {
                    _logger.LogWarning("ContainerType {Id} not found for update", id);
                    return Result<ContainerTypeResponse>.NotFound("Container type not found.");
                }

                var oldContainerType = containerType;
                var existing = await _unitOfWork.ContainerTypes.GetAllAsync(ct => !ct.IsDeleted && ct.Name == dto.Name && ct.Id != id);
                if (existing.Any())
                    return Result<ContainerTypeResponse>.Failure($"A container type with name '{dto.Name}' already exists.");

                containerType.Name = dto.Name;
                containerType.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = containerType.Id,
                    EntityName = nameof(ContainerType).ToUpper(), Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldContainerType), NewValues = JsonSerializer.Serialize(containerType), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = containerType.Name, EntityId = containerType.Id, Type = AliasType.ContainerType });

                _logger.LogInformation("ContainerType {Id} updated successfully", id);
                return Result<ContainerTypeResponse>.Success(_mapper.Map<ContainerTypeResponse>(containerType));
            });
        }

        public async Task<Result> DeleteAsync(Guid id, string userId)
        {
            _logger.LogInformation("Deleting container type {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(id);
                if (containerType == null || containerType.IsDeleted)
                {
                    _logger.LogWarning("ContainerType {Id} not found for deletion", id);
                    return Result.NotFound("Container type not found.");
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = containerType.Id,
                    EntityName = nameof(ContainerType).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(containerType), NewValues = "Deleted", UserId = userId
                };

                containerType.IsDeleted = true;
                containerType.DeletedAt = DateTimeOffset.UtcNow;
                containerType.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("ContainerType {Id} deleted", id);
                return Result.Success();
            });
        }

        private async Task<Result<T>> ExecuteInTransactionAsync<T>(Func<Task<Result<T>>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(ContainerTypeService));
                throw;
            }
        }

        private async Task<Result> ExecuteInTransactionAsync(Func<Task<Result>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(ContainerTypeService));
                throw;
            }
        }
    }
}
