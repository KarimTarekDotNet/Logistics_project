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
    public class PortService : IPortService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasService _aliasService;
        private readonly IMapper _mapper;
        private readonly ILogger<PortService> _logger;

        public PortService(IUnitOfWork unitOfWork, IMapper mapper, IAliasService aliasService, ILogger<PortService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aliasService = aliasService;
            _logger = logger;
        }

        public async Task<Result<PortResponse>> GetByIdAsync(Guid id)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(id);
            if (port == null || port.IsDeleted)
            {
                _logger.LogWarning("Port {Id} not found", id);
                return Result<PortResponse>.NotFound("Port not found.");
            }
            return Result<PortResponse>.Success(_mapper.Map<PortResponse>(port));
        }

        public async Task<Result<IEnumerable<PortResponse>>> GetAllAsync(QueryParameters query)
        {
            var ports = await _unitOfWork.Ports.GetAllAsync(query);
            return Result<IEnumerable<PortResponse>>.Success(_mapper.Map<IEnumerable<PortResponse>>(ports.Where(p => !p.IsDeleted)));
        }

        public async Task<Result<IEnumerable<PortResponse>>> GetByCountryAsync(string country, QueryParameters query)
        {
            var ports = await _unitOfWork.Ports.GetByCountryAsync(country, query);
            return Result<IEnumerable<PortResponse>>.Success(_mapper.Map<IEnumerable<PortResponse>>(ports.Where(p => !p.IsDeleted)));
        }

        public async Task<Result<PortResponse>> CreateAsync(CreatePortRequest dto, string userId)
        {
            _logger.LogInformation("Creating port {Code} by user {UserId}", dto.Code, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                dto.Code = dto.Code.Replace(" ", "").Trim().ToUpper();
                var existing = await _unitOfWork.Ports.GetByCodeAsync(dto.Code);
                if (existing != null && !existing.IsDeleted)
                {
                    _logger.LogWarning("Port code {Code} already exists", dto.Code);
                    return Result<PortResponse>.Failure($"A port with code '{dto.Code}' already exists.");
                }

                var port = _mapper.Map<Port>(dto);
                port.CreatedAt = DateTimeOffset.UtcNow;
                port.IsDeleted = false;

                var audit = new AuditLog
                {
                    CreatedAt = port.CreatedAt, EntityId = port.Id,
                    EntityName = nameof(Port).ToUpper(), Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null, NewValues = JsonSerializer.Serialize(port), UserId = userId
                };

                await _unitOfWork.Ports.AddAsync(port);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = port.Name, EntityId = port.Id, Type = AliasType.Port });
                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = port.Code, EntityId = port.Id, Type = AliasType.Port });

                _logger.LogInformation("Port {Id} created successfully", port.Id);
                return Result<PortResponse>.Success(_mapper.Map<PortResponse>(port), 201);
            });
        }

        public async Task<Result<PortResponse>> UpdateAsync(Guid id, UpdatePortRequest dto, string userId)
        {
            _logger.LogInformation("Updating port {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var port = await _unitOfWork.Ports.GetByIdAsync(id);
                if (port == null || port.IsDeleted)
                {
                    _logger.LogWarning("Port {Id} not found for update", id);
                    return Result<PortResponse>.NotFound("Port not found.");
                }

                var oldPort = port;
                if (string.IsNullOrWhiteSpace(dto.Name)) dto.Name = port.Name;
                if (string.IsNullOrWhiteSpace(dto.Code)) dto.Code = port.Code;
                if (string.IsNullOrWhiteSpace(dto.Country)) dto.Country = port.Country;

                var existing = await _unitOfWork.Ports.GetByCodeAsync(dto.Code);
                if (existing != null && !existing.IsDeleted && existing.Id != id)
                    return Result<PortResponse>.Failure($"A port with code '{dto.Code}' already exists.");

                port.Name = dto.Name;
                port.Code = dto.Code.Replace(" ", "").Trim().ToUpper();
                port.Country = dto.Country;
                port.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = port.Id,
                    EntityName = nameof(Port).ToUpper(), Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldPort), NewValues = JsonSerializer.Serialize(port), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = port.Name, EntityId = port.Id, Type = AliasType.Port });
                await _aliasService.CreateAsync(new CreateAliasRequest { AliasName = port.Code, EntityId = port.Id, Type = AliasType.Port });

                _logger.LogInformation("Port {Id} updated successfully", id);
                return Result<PortResponse>.Success(_mapper.Map<PortResponse>(port));
            });
        }

        public async Task<Result> DeleteAsync(Guid id, string userId)
        {
            _logger.LogInformation("Deleting port {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var port = await _unitOfWork.Ports.GetByIdAsync(id);
                if (port == null || port.IsDeleted)
                {
                    _logger.LogWarning("Port {Id} not found for deletion", id);
                    return Result.NotFound("Port not found.");
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = port.Id,
                    EntityName = nameof(Port).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(port), NewValues = "Deleted", UserId = userId
                };

                port.IsDeleted = true;
                port.DeletedAt = DateTimeOffset.UtcNow;
                port.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Port {Id} deleted", id);
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(PortService));
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(PortService));
                throw;
            }
        }
    }
}
