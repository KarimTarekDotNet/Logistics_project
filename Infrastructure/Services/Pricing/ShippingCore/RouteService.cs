using Application.ApplicationRules;
using Application.DTOs.ShippingCore;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.ShippingCore;
using Domain.Exceptions;
using Infrastructure.Helper;
using System.Text.Json;

namespace Infrastructure.Services.Pricing.ShippingCore
{
    public class RouteService : IRouteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RouteService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<RouteResponse?> GetByIdAsync(Guid id)
        {
            var route = await _unitOfWork.Routes.GetWithPortsAsync(id);
            if (route == null || route.IsDeleted)
                return null;

            return _mapper.Map<RouteResponse>(route);
        }

        public async Task<IEnumerable<RouteResponse>> GetAllAsync(QueryParameters query)
        {
            var routes = await _unitOfWork.Routes.GetAllAsync(query);
            return _mapper.Map<IEnumerable<RouteResponse>>(routes);
        }

        public async Task<IEnumerable<RouteResponse>> GetByFromPortAsync(Guid fromPortId, QueryParameters query)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(fromPortId);
            if (port == null || port.IsDeleted)
                throw new KeyNotFoundException("Port not found.");

            var routes = await _unitOfWork.Routes.GetByFromPortAsync(fromPortId, query);
            return _mapper.Map<IEnumerable<RouteResponse>>(routes.Where(r => !r.IsDeleted));
        }

        public async Task<IEnumerable<RouteResponse>> GetByToPortAsync(Guid toPortId, QueryParameters query)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(toPortId);
            if (port == null || port.IsDeleted)
                throw new KeyNotFoundException("Port not found.");

            var routes = await _unitOfWork.Routes.GetByToPortAsync(toPortId, query);
            return _mapper.Map<IEnumerable<RouteResponse>>(routes.Where(r => !r.IsDeleted));
        }

        public async Task<RouteResponse> CreateAsync(CreateRouteRequest dto, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                if (!PortRules.AreDistinct(dto.FromPortId, dto.ToPortId))
                    throw new ArgumentException("Origin and destination ports must be different.");

                var fromPort = await _unitOfWork.Ports.GetByIdAsync(dto.FromPortId);
                if (fromPort == null || fromPort.IsDeleted)
                    throw new KeyNotFoundException("Origin port not found.");

                var toPort = await _unitOfWork.Ports.GetByIdAsync(dto.ToPortId);
                if (toPort == null || toPort.IsDeleted)
                    throw new KeyNotFoundException("Destination port not found.");

                var existing = await _unitOfWork.Routes.GetByPortsAsync(dto.FromPortId, dto.ToPortId);
                if (existing != null && !existing.IsDeleted)
                    throw new BusinessRuleException("A route between these two ports already exists.");

                var route = _mapper.Map<Route>(dto);
                route.CreatedAt = DateTimeOffset.UtcNow;
                route.UpdatedAt = null;
                route.IsDeleted = false;
                route.DeletedAt = null;

                var audit = new AuditLog
                {
                    CreatedAt = route.CreatedAt,
                    EntityId = route.Id,
                    EntityName = nameof(Route).ToUpper(),
                    Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null,
                    NewValues = JsonSerializer.Serialize(route),
                    UserId = userId
                };

                await _unitOfWork.Routes.AddAsync(route);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                var created = await _unitOfWork.Routes.GetWithPortsAsync(route.Id);
                return _mapper.Map<RouteResponse>(created);
            });
        }

        public async Task<RouteResponse> UpdateAsync(Guid id, UpdateRouteRequest dto, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var route = await _unitOfWork.Routes.GetByIdAsync(id);
                if (route == null || route.IsDeleted)
                    throw new KeyNotFoundException("Route not found.");

                var oldRoute = route;

                if (!PortRules.AreDistinct(dto.FromPortId, dto.ToPortId))
                    throw new BusinessRuleException("Origin and destination ports must be different.");

                var fromPort = await _unitOfWork.Ports.GetByIdAsync(dto.FromPortId);
                if (fromPort == null || fromPort.IsDeleted)
                    throw new KeyNotFoundException("Origin port not found.");

                var toPort = await _unitOfWork.Ports.GetByIdAsync(dto.ToPortId);
                if (toPort == null || toPort.IsDeleted)
                    throw new KeyNotFoundException("Destination port not found.");

                var existing = await _unitOfWork.Routes.GetByPortsAsync(dto.FromPortId, dto.ToPortId);
                if (existing != null && !existing.IsDeleted && existing.Id != id)
                    throw new BusinessRuleException("A route between these two ports already exists.");

                route.FromPortId = dto.FromPortId;
                route.ToPortId = dto.ToPortId;
                route.UpdatedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = route.Id,
                    EntityName = nameof(Route).ToUpper(),
                    Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldRoute),
                    NewValues = JsonSerializer.Serialize(route),
                    UserId = userId
                };

                _unitOfWork.Routes.Update(route);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                var updated = await _unitOfWork.Routes.GetWithPortsAsync(route.Id);
                return _mapper.Map<RouteResponse>(updated);
            });
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                var route = await _unitOfWork.Routes.GetByIdAsync(id);
                if (route == null || route.IsDeleted)
                    throw new KeyNotFoundException("Route not found.");

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = route.Id,
                    EntityName = nameof(Route).ToUpper(),
                    Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(route),
                    NewValues = "Deleted",
                    UserId = userId
                };

                route.IsDeleted = true;
                route.DeletedAt = DateTimeOffset.UtcNow;
                route.UpdatedAt = DateTimeOffset.UtcNow;

                _unitOfWork.Routes.Update(route);
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