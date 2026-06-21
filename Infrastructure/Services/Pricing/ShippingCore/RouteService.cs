using Application.ApplicationRules;
using Application.Common;
using Application.DTOs.ShippingCore;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.ShippingCore;
using Infrastructure.Helper;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Pricing.ShippingCore
{
    public class RouteService : IRouteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RouteService> _logger;

        public RouteService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RouteService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<RouteResponse>> GetByIdAsync(Guid id)
        {
            var route = await _unitOfWork.Routes.GetWithPortsAsync(id);
            if (route == null || route.IsDeleted)
            {
                _logger.LogWarning("Route {Id} not found", id);
                return Result<RouteResponse>.NotFound("Route not found.");
            }
            return Result<RouteResponse>.Success(_mapper.Map<RouteResponse>(route));
        }

        public async Task<Result<IEnumerable<RouteResponse>>> GetAllAsync(QueryParameters query)
        {
            var routes = await _unitOfWork.Routes.GetAllAsync(query);
            return Result<IEnumerable<RouteResponse>>.Success(_mapper.Map<IEnumerable<RouteResponse>>(routes));
        }

        public async Task<Result<IEnumerable<RouteResponse>>> GetByFromPortAsync(Guid fromPortId, QueryParameters query)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(fromPortId);
            if (port == null || port.IsDeleted)
                return Result<IEnumerable<RouteResponse>>.NotFound("Port not found.");

            var routes = await _unitOfWork.Routes.GetByFromPortAsync(fromPortId, query);
            return Result<IEnumerable<RouteResponse>>.Success(_mapper.Map<IEnumerable<RouteResponse>>(routes.Where(r => !r.IsDeleted)));
        }

        public async Task<Result<IEnumerable<RouteResponse>>> GetByToPortAsync(Guid toPortId, QueryParameters query)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(toPortId);
            if (port == null || port.IsDeleted)
                return Result<IEnumerable<RouteResponse>>.NotFound("Port not found.");

            var routes = await _unitOfWork.Routes.GetByToPortAsync(toPortId, query);
            return Result<IEnumerable<RouteResponse>>.Success(_mapper.Map<IEnumerable<RouteResponse>>(routes.Where(r => !r.IsDeleted)));
        }

        public async Task<Result<RouteResponse>> CreateAsync(CreateRouteRequest dto, string userId)
        {
            _logger.LogInformation("Creating route {From}->{To} by user {UserId}", dto.FromPortId, dto.ToPortId, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                if (!PortRules.AreDistinct(dto.FromPortId, dto.ToPortId))
                    return Result<RouteResponse>.Failure("Origin and destination ports must be different.");

                var fromPort = await _unitOfWork.Ports.GetByIdAsync(dto.FromPortId);
                if (fromPort == null || fromPort.IsDeleted)
                    return Result<RouteResponse>.NotFound("Origin port not found.");

                var toPort = await _unitOfWork.Ports.GetByIdAsync(dto.ToPortId);
                if (toPort == null || toPort.IsDeleted)
                    return Result<RouteResponse>.NotFound("Destination port not found.");

                var existing = await _unitOfWork.Routes.GetByPortsAsync(dto.FromPortId, dto.ToPortId);
                if (existing != null && !existing.IsDeleted)
                    return Result<RouteResponse>.Failure("A route between these two ports already exists.");

                var route = _mapper.Map<Route>(dto);
                route.CreatedAt = DateTimeOffset.UtcNow;
                route.UpdatedAt = null;
                route.IsDeleted = false;
                route.DeletedAt = null;

                var audit = new AuditLog
                {
                    CreatedAt = route.CreatedAt, EntityId = route.Id,
                    EntityName = nameof(Route).ToUpper(), Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null, NewValues = JsonSerializer.Serialize(route), UserId = userId
                };

                await _unitOfWork.Routes.AddAsync(route);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                var created = await _unitOfWork.Routes.GetWithPortsAsync(route.Id);
                _logger.LogInformation("Route {Id} created successfully", route.Id);
                return Result<RouteResponse>.Success(_mapper.Map<RouteResponse>(created), 201);
            });
        }

        public async Task<Result<RouteResponse>> UpdateAsync(Guid id, UpdateRouteRequest dto, string userId)
        {
            _logger.LogInformation("Updating route {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var route = await _unitOfWork.Routes.GetByIdAsync(id);
                if (route == null || route.IsDeleted)
                {
                    _logger.LogWarning("Route {Id} not found for update", id);
                    return Result<RouteResponse>.NotFound("Route not found.");
                }

                var oldRoute = route;
                if (!PortRules.AreDistinct(dto.FromPortId, dto.ToPortId))
                    return Result<RouteResponse>.Failure("Origin and destination ports must be different.");

                var fromPort = await _unitOfWork.Ports.GetByIdAsync(dto.FromPortId);
                if (fromPort == null || fromPort.IsDeleted)
                    return Result<RouteResponse>.NotFound("Origin port not found.");

                var toPort = await _unitOfWork.Ports.GetByIdAsync(dto.ToPortId);
                if (toPort == null || toPort.IsDeleted)
                    return Result<RouteResponse>.NotFound("Destination port not found.");

                var existing = await _unitOfWork.Routes.GetByPortsAsync(dto.FromPortId, dto.ToPortId);
                if (existing != null && !existing.IsDeleted && existing.Id != id)
                    return Result<RouteResponse>.Failure("A route between these two ports already exists.");

                route.FromPortId = dto.FromPortId;
                route.ToPortId = dto.ToPortId;
                route.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = route.Id,
                    EntityName = nameof(Route).ToUpper(), Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldRoute), NewValues = JsonSerializer.Serialize(route), UserId = userId
                };

                _unitOfWork.Routes.Update(route);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                var updated = await _unitOfWork.Routes.GetWithPortsAsync(route.Id);
                _logger.LogInformation("Route {Id} updated successfully", id);
                return Result<RouteResponse>.Success(_mapper.Map<RouteResponse>(updated));
            });
        }

        public async Task<Result> DeleteAsync(Guid id, string userId)
        {
            _logger.LogInformation("Deleting route {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var route = await _unitOfWork.Routes.GetByIdAsync(id);
                if (route == null || route.IsDeleted)
                {
                    _logger.LogWarning("Route {Id} not found for deletion", id);
                    return Result.NotFound("Route not found.");
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = route.Id,
                    EntityName = nameof(Route).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(route), NewValues = "Deleted", UserId = userId
                };

                route.IsDeleted = true;
                route.DeletedAt = DateTimeOffset.UtcNow;
                route.UpdatedAt = DateTimeOffset.UtcNow;

                _unitOfWork.Routes.Update(route);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Route {Id} deleted", id);
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(RouteService));
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(RouteService));
                throw;
            }
        }
    }
}
