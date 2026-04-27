using Application.ApplicationRules;
using Application.DTOs.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Models;
using AutoMapper;
using Domain.Entities.Pricing.PricingEngine;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Pricing.PricingEngine
{
    public class RateService : IRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RateService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<RateResponse> CreateAsync(CreateRateRequest dto)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                await EnsureReferencesExistAsync(dto.CarrierId, dto.RouteId, dto.ContainerTypeId);

                var rate = _mapper.Map<Rate>(dto);
                rate.IsActive = RateRules.ShouldBeActive(rate.ValidFrom, rate.ValidTo);
                rate.CreatedAt = DateTimeOffset.UtcNow;

                if (rate.IsActive)
                {
                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId, rate.ContainerTypeId);
                }

                await _unitOfWork.Rates.AddAsync(rate);
                await _unitOfWork.SaveChangesAsync();

                var createdRate = await GetRateWithDetailsOrThrowAsync(rate.Id);

                return _mapper.Map<RateResponse>(createdRate);
            });
        }

        public async Task DeleteAsync(Guid id)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                var rate = await _unitOfWork.Rates.GetByIdAsync(id);
                if (rate == null)
                    throw new KeyNotFoundException("Rate not found.");

                if (rate.IsDeleted)
                    throw new InvalidOperationException("Rate is already deleted.");

                rate.IsDeleted = true;
                rate.IsActive = false;
                rate.DeletedAt = DateTimeOffset.UtcNow;

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.SaveChangesAsync();

                return true;
            });
        }

        public async Task<RateResponse> UpdateAsync(Guid id, UpdateRateRequest dto)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await GetRateWithDetailsOrThrowAsync(id);

                if(string.IsNullOrEmpty(dto.Currency))
                    dto.Currency = rate.Currency;

                if (dto.Price <= 0)
                    dto.Price = rate.Price;

                if (dto.ValidFrom == default)
                    dto.ValidFrom = rate.ValidFrom;

                if (dto.ValidTo == default)
                    dto.ValidTo = rate.ValidTo;

                if(!RateRules.IsValidDateRange(dto.ValidFrom, dto.ValidTo))
                    throw new InvalidOperationException("Invalid date range.");

                if(!RateRules.IsValidCurrency(dto.Currency))
                    throw new InvalidOperationException($"Invalid currency please choose a valid currency" +
                        $" from the allowed list [{string.Join(", ", RateRules.AllowedCurrencies)}].");
                rate.Price = dto.Price; rate.Currency = dto.Currency; rate.ValidFrom = dto.ValidFrom;
                rate.ValidTo = dto.ValidTo; rate.UpdatedAt = DateTimeOffset.UtcNow;

                var shouldBeActive = RateRules.ShouldBeActive(rate.ValidFrom, rate.ValidTo);
                rate.IsActive = shouldBeActive;

                if (shouldBeActive)
                {
                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId,
                    rate.ContainerTypeId, rate.Id);
                }

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.SaveChangesAsync();

                return _mapper.Map<RateResponse>(rate);
            });
        }

        public async Task<IEnumerable<RateResponse>> SearchAsync(RateParameters query)
        {
            var rates = await _unitOfWork.Rates.SearchAsync(query);
            return _mapper.Map<IEnumerable<RateResponse>>(rates);
        }

        public async Task<RateResponse?> GetByIdAsync(Guid id)
        {
            var rate = await _unitOfWork.Rates.GetByIdAsync(id, IncludeRateDetails());

            return rate == null ? null : _mapper.Map<RateResponse>(rate);
        }

        public async Task<bool> ChangeRateActive(Guid rateId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await _unitOfWork.Rates.GetByIdAsync(rateId);
                if (rate == null)
                    throw new KeyNotFoundException("Rate not found.");

                if (rate.IsDeleted)
                    throw new InvalidOperationException("Cannot change active state of a deleted rate.");

                if (!rate.IsActive)
                {
                    if (!RateRules.CanActivateRate(rate.ValidTo))
                        throw new InvalidOperationException("Cannot activate a rate with an expired validity period.");

                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId,
                    rate.ContainerTypeId, rate.Id);

                    rate.IsActive = true;
                }
                else
                {
                    rate.IsActive = false;
                }

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.SaveChangesAsync();

                return rate.IsActive;
            });
        }

        private async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> action)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var result = await action();
                await _unitOfWork.CommitTransactionAsync();
                return result;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        private async Task EnsureReferencesExistAsync(Guid carrierId, Guid routeId, Guid containerTypeId)
        {
            var carrier = await _unitOfWork.Carriers.GetByIdAsync(carrierId);
            if (carrier == null)
                throw new KeyNotFoundException("Carrier not found.");

            var route = await _unitOfWork.Routes.GetByIdAsync(routeId);
            if (route == null)
                throw new KeyNotFoundException("Route not found.");

            var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(containerTypeId);
            if (containerType == null)
                throw new KeyNotFoundException("Container type not found.");
        }

        private async Task<Rate> GetRateWithDetailsOrThrowAsync(Guid rateId)
        {
            var rate = await _unitOfWork.Rates.GetByIdAsync(rateId, IncludeRateDetails());

            if (rate == null)
                throw new KeyNotFoundException("Rate not found.");

            return rate;
        }

        private static Func<IQueryable<Rate>, IQueryable<Rate>> IncludeRateDetails()
        {
            return query => query
                .Include(r => r.Carrier)
                .Include(r => r.Route)
                    .ThenInclude(r => r.FromPort)
                .Include(r => r.Route)
                    .ThenInclude(r => r.ToPort)
                .Include(r => r.ContainerType);
        }

        private async Task DeactivateOtherActiveRatesAsync(Guid carrierId, Guid routeId, Guid containerTypeId, Guid? excludeRateId = null)
        {
            var activeRates = await _unitOfWork.Rates
                .GetActiveRatesByCarrierRouteAndContainerTypeAsync(carrierId, routeId, containerTypeId);

            foreach (var activeRate in activeRates)
            {
                if (excludeRateId.HasValue && activeRate.Id == excludeRateId.Value)
                    continue;

                activeRate.IsActive = false;
                _unitOfWork.Rates.Update(activeRate);
            }
        }
    }
}