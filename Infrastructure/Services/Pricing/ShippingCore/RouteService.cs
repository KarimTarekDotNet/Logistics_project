using Application.ApplicationRules;
using Application.DTOs.ShippingCore;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Models;
using AutoMapper;
using Domain.Entities.ShippingCore;
using Domain.Exceptions;

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

        public async Task<RouteResponse> CreateAsync(CreateRouteRequest dto)
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

            await _unitOfWork.Routes.AddAsync(route);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Routes.GetWithPortsAsync(route.Id);
            return _mapper.Map<RouteResponse>(created);
        }

        public async Task<RouteResponse> UpdateAsync(Guid id, UpdateRouteRequest dto)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(id);
            if (route == null || route.IsDeleted)
                throw new KeyNotFoundException("Route not found.");

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

            _unitOfWork.Routes.Update(route);
            await _unitOfWork.SaveChangesAsync();

            var updated = await _unitOfWork.Routes.GetWithPortsAsync(route.Id);
            return _mapper.Map<RouteResponse>(updated);
        }

        public async Task DeleteAsync(Guid id)
        {
            var route = await _unitOfWork.Routes.GetByIdAsync(id);
            if (route == null || route.IsDeleted)
                throw new KeyNotFoundException("Route not found.");

            route.IsDeleted = true;
            route.DeletedAt = DateTimeOffset.UtcNow;
            route.UpdatedAt = DateTimeOffset.UtcNow;

            _unitOfWork.Routes.Update(route);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}