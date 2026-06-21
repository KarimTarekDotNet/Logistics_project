using Application.ApplicationRules;
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
    public class CarrierService : ICarrierService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasService _aliasService;
        private readonly IMapper _mapper;
        private readonly ILogger<CarrierService> _logger;

        public CarrierService(IUnitOfWork unitOfWork, IMapper mapper, IAliasService aliasService, ILogger<CarrierService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aliasService = aliasService;
            _logger = logger;
        }

        public async Task<Result<CarrierResponse>> GetByIdAsync(Guid id)
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
            if (carrier == null || carrier.IsDeleted)
            {
                _logger.LogWarning("Carrier {CarrierId} not found", id);
                return Result<CarrierResponse>.NotFound("Carrier not found.");
            }
            return Result<CarrierResponse>.Success(_mapper.Map<CarrierResponse>(carrier));
        }

        public async Task<Result<CarrierResponse>> GetByNameOrCodeAsync(string input)
        {
            var carrier = await _unitOfWork.Carriers.GetByNameOrCodeAsync(input);
            if (carrier == null)
            {
                var resolved = await _aliasService.ResolveAsync(input, AliasType.Carrier);
                if (resolved is { Resolved: true, EntityId: not null })
                    carrier = await _unitOfWork.Carriers.GetByIdAsync(resolved.EntityId.Value);
            }
            return Result<CarrierResponse>.Success(_mapper.Map<CarrierResponse>(carrier));
        }

        public async Task<Result<IEnumerable<CarrierResponse>>> GetAllAsync(QueryParameters query)
        {
            var carriers = await _unitOfWork.Carriers.GetAllAsync(query);
            return Result<IEnumerable<CarrierResponse>>.Success(_mapper.Map<IEnumerable<CarrierResponse>>(carriers));
        }

        public async Task<Result<CarrierResponse>> CreateAsync(CreateCarrierRequest dto, string userId)
        {
            _logger.LogInformation("Creating carrier {Code} by user {UserId}", dto.Code, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var existing = await _unitOfWork.Carriers.GetByNameOrCodeAsync(dto.Code);
                if (existing != null && !existing.IsDeleted)
                {
                    _logger.LogWarning("Carrier code {Code} already exists", dto.Code);
                    return Result<CarrierResponse>.Failure($"A carrier with code '{dto.Code}' already exists.");
                }

                var carrier = _mapper.Map<Carrier>(dto);
                carrier.CreatedAt = DateTimeOffset.UtcNow;
                carrier.IsDeleted = false;

                var audit = new AuditLog
                {
                    CreatedAt = carrier.CreatedAt,
                    EntityId = carrier.Id,
                    EntityName = nameof(Carrier).ToUpper(),
                    Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null,
                    NewValues = JsonSerializer.Serialize(carrier),
                    UserId = userId
                };

                await _unitOfWork.Carriers.AddAsync(carrier);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = carrier.Name, EntityId = carrier.Id, Type = AliasType.Carrier });
                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = carrier.Code, EntityId = carrier.Id, Type = AliasType.Carrier });

                _logger.LogInformation("Carrier {CarrierId} created successfully", carrier.Id);
                return Result<CarrierResponse>.Success(_mapper.Map<CarrierResponse>(carrier), 201);
            });
        }

        public async Task<Result<CarrierResponse>> UpdateAsync(Guid id, UpdateCarrierRequest dto, string userId)
        {
            _logger.LogInformation("Updating carrier {CarrierId} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
                if (carrier == null || carrier.IsDeleted)
                {
                    _logger.LogWarning("Carrier {CarrierId} not found for update", id);
                    return Result<CarrierResponse>.NotFound("Carrier not found.");
                }

                var oldCarrier = carrier;
                if (string.IsNullOrWhiteSpace(dto.Code)) dto.Code = carrier.Code;
                if (!CarrierRule.IsCodeMatch(dto.Code))
                    return Result<CarrierResponse>.Failure("Carrier code must be exactly 4 uppercase letters (SCAC format).");

                var existing = await _unitOfWork.Carriers.GetByNameOrCodeAsync(dto.Code);
                if (existing != null && !existing.IsDeleted && existing.Id != id)
                    return Result<CarrierResponse>.Failure($"A carrier with code '{dto.Code}' already exists.");

                if (string.IsNullOrEmpty(dto.Name)) dto.Name = carrier.Name;
                carrier.Name = dto.Name;
                carrier.Code = dto.Code;
                carrier.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = carrier.Id,
                    EntityName = nameof(Carrier).ToUpper(), Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldCarrier), NewValues = JsonSerializer.Serialize(carrier),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = carrier.Name, EntityId = carrier.Id, Type = AliasType.Carrier });
                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = carrier.Code, EntityId = carrier.Id, Type = AliasType.Carrier });

                _logger.LogInformation("Carrier {CarrierId} updated successfully", carrier.Id);
                return Result<CarrierResponse>.Success(_mapper.Map<CarrierResponse>(carrier));
            });
        }

        public async Task<Result> DeleteAsync(Guid id, string userId)
        {
            _logger.LogInformation("Deleting carrier {CarrierId} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var carrier = await _unitOfWork.Carriers.GetByIdAsync(id);
                if (carrier == null || carrier.IsDeleted)
                {
                    _logger.LogWarning("Carrier {CarrierId} not found for deletion", id);
                    return Result.NotFound("Carrier not found.");
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = carrier.Id,
                    EntityName = nameof(Carrier).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(carrier), NewValues = "Deleted", UserId = userId
                };

                carrier.IsDeleted = true;
                carrier.DeletedAt = DateTimeOffset.UtcNow;
                carrier.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Carrier {CarrierId} deleted", id);
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(CarrierService));
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(CarrierService));
                throw;
            }
        }
    }
}
