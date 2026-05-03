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
    public class PortService : IPortService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PortService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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

        public async Task<PortResponse> CreateAsync(CreatePortRequest dto)
        {
            var existing = await _unitOfWork.Ports.GetByCodeAsync(dto.Code);
            if (existing != null && !existing.IsDeleted)
                throw new BusinessRuleException($"A port with code '{dto.Code}' already exists.");

            dto.Code = dto.Code.Replace(" ", "").Trim().ToUpper();

            var port = _mapper.Map<Port>(dto);
            port.CreatedAt = DateTimeOffset.UtcNow;
            port.UpdatedAt = null;
            port.IsDeleted = false;
            port.DeletedAt = null;

            await _unitOfWork.Ports.AddAsync(port);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PortResponse>(port);
        }

        public async Task<PortResponse> UpdateAsync(Guid id, UpdatePortRequest dto)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(id);
            if (port == null || port.IsDeleted)
                throw new KeyNotFoundException("Port not found.");

            if(string.IsNullOrWhiteSpace(dto.Name))
                dto.Name = port.Name;

            if(string.IsNullOrWhiteSpace(dto.Code))
                dto.Code = port.Code;

            if(string.IsNullOrWhiteSpace(dto.Country))
                dto.Country = port.Country;

            var existing = await _unitOfWork.Ports.GetByCodeAsync(dto.Code);
            if (existing != null && !existing.IsDeleted && existing.Id != id)
                throw new BusinessRuleException($"A port with code '{dto.Code}' already exists.");

            port.Name = dto.Name;
            port.Code = dto.Code.Replace(" ", "").Trim().ToUpper();
            port.Country = dto.Country;
            port.UpdatedAt = DateTimeOffset.UtcNow;

            _unitOfWork.Ports.Update(port);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<PortResponse>(port);
        }

        public async Task DeleteAsync(Guid id)
        {
            var port = await _unitOfWork.Ports.GetByIdAsync(id);
            if (port == null || port.IsDeleted)
                throw new KeyNotFoundException("Port not found.");

            port.IsDeleted = true;
            port.DeletedAt = DateTimeOffset.UtcNow;
            port.UpdatedAt = DateTimeOffset.UtcNow;

            _unitOfWork.Ports.Update(port);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}