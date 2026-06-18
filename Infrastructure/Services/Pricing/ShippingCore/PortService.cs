using Application.ApplicationRules;
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
using Domain.Exceptions;
using Infrastructure.Helper;
using System.Text.Json;

namespace Infrastructure.Services.Pricing.ShippingCore
{
    public class PortService : IPortService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAliasService _aliasService;
        private readonly IMapper _mapper;

        public PortService(IUnitOfWork unitOfWork, IMapper mapper, IAliasService aliasService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _aliasService = aliasService;
        }

        public async Task<PortResponse?> GetByIdAsync(Guid id)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(id);
            if (port == null || port.IsDeleted)
                return null;

            return _mapper.Map<PortResponse>(port);
        }

        public async Task<IEnumerable<PortResponse>> GetAllAsync(QueryParameters query)
        {
            var ports = await _unitOfWork.Ports.GetAllAsync(query);
            return _mapper.Map<IEnumerable<PortResponse>>(ports.Where(p => !p.IsDeleted));
        }

        public async Task<IEnumerable<PortResponse>> GetByCountryAsync(string country, QueryParameters query)
        {
            var ports = await _unitOfWork.Ports.GetByCountryAsync(country, query);
            return _mapper.Map<IEnumerable<PortResponse>>(ports.Where(p => !p.IsDeleted));
        }

        public async Task<PortResponse> CreateAsync(CreatePortRequest dto, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                dto.Code = dto.Code.Replace(" ", "").Trim().ToUpper();
                var existing = await _unitOfWork.Ports.GetByCodeAsync(dto.Code);
                if (existing != null && !existing.IsDeleted)
                    throw new BusinessRuleException($"A port with code '{dto.Code}' already exists.");

                var port = _mapper.Map<Port>(dto);
                port.CreatedAt = DateTimeOffset.UtcNow;
                port.IsDeleted = false;

                var audit = new AuditLog
                {
                    CreatedAt = port.CreatedAt,
                    EntityId = port.Id,
                    EntityName = nameof(Port).ToUpper(),
                    Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null,
                    NewValues = JsonSerializer.Serialize(port),
                    UserId = userId
                };

                await _unitOfWork.Ports.AddAsync(port);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                await _aliasService.CreateAsync(new CreateAliasRequest
                {
                    AliasName = port.Name,
                    EntityId = port.Id,
                    Type = AliasType.Port
                });

                await _aliasService.CreateAsync(new CreateAliasRequest
                {
                    AliasName = port.Code,
                    EntityId = port.Id,
                    Type = AliasType.Port
                });

                return _mapper.Map<PortResponse>(port);
            });
        }

        public async Task<PortResponse> UpdateAsync(Guid id, UpdatePortRequest dto, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var port = await _unitOfWork.Ports.GetByIdAsync(id);
                if (port == null || port.IsDeleted)
                    throw new KeyNotFoundException("Port not found.");

                var oldPort = port;

                if (string.IsNullOrWhiteSpace(dto.Name))
                    dto.Name = port.Name;

                if (string.IsNullOrWhiteSpace(dto.Code))
                    dto.Code = port.Code;

                if (string.IsNullOrWhiteSpace(dto.Country))
                    dto.Country = port.Country;

                var existing = await _unitOfWork.Ports.GetByCodeAsync(dto.Code);
                if (existing != null && !existing.IsDeleted && existing.Id != id)
                    throw new BusinessRuleException($"A port with code '{dto.Code}' already exists.");

                port.Name = dto.Name;
                port.Code = dto.Code.Replace(" ", "").Trim().ToUpper();
                port.Country = dto.Country;
                port.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = port.Id,
                    EntityName = nameof(Port).ToUpper(),
                    Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldPort),
                    NewValues = JsonSerializer.Serialize(port),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                await _aliasService.CreateAsync(new CreateAliasRequest
                {
                    AliasName = port.Name,
                    EntityId = port.Id,
                    Type = AliasType.Port
                });

                await _aliasService.CreateAsync(new CreateAliasRequest
                {
                    AliasName = port.Code,
                    EntityId = port.Id,
                    Type = AliasType.Port
                });

                return _mapper.Map<PortResponse>(port);
            });
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                var port = await _unitOfWork.Ports.GetByIdAsync(id);
                if (port == null || port.IsDeleted)
                    throw new KeyNotFoundException("Port not found.");

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = port.Id,
                    EntityName = nameof(Port).ToUpper(),
                    Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(port),
                    NewValues = "Deleted",
                    UserId = userId
                };

                port.IsDeleted = true;
                port.DeletedAt = DateTimeOffset.UtcNow;
                port.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }

        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }
    }
}