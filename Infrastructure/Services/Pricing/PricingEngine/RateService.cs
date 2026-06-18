using Application.ApplicationRules;
using Application.DTOs.Pricing.PricingEngine.Rates;
using Application.DTOs.Pricing.Recommendations;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Models;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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

        public async Task<int> CountAsync()
        {
            var count = await _unitOfWork.Rates.CountAsync();
            if (count >= 0)
                return count.Value;

            else
                return 0;
        }

        public async Task<RateResponse> CreateAsync(CreateRateRequest dto, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                await EnsureReferencesExistAsync(dto.CarrierId, dto.RouteId, dto.ContainerTypeId);

                ValidateRateConstraints(
                dto.MaxGrossWeightKg,
                dto.MaxNetWeightKg,
                dto.MaxVolumeCbm,
                dto.MinTemperatureCelsius,
                dto.MaxTemperatureCelsius);

                var rate = _mapper.Map<Rate>(dto);
                rate.IsActive = RateRules.ShouldBeActive(rate.ValidFrom, rate.ValidTo);
                rate.CreatedAt = DateTimeOffset.UtcNow;

                var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(dto.ContainerTypeId);

                if (containerType == null)
                    throw new KeyNotFoundException("Container type not found.");

                if (dto.AllowsHazardous == true && !containerType.Name
                .Contains("Reefer", StringComparison.OrdinalIgnoreCase))
                {
                    throw new BusinessRuleException("Temperature-controlled cargo requires a reefer container.");
                }

                if (rate.IsActive)
                {
                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId,
                    rate.ContainerTypeId, dto.ValidFrom, dto.ValidTo);
                }

                var audit = new AuditLog
                {
                    CreatedAt = rate.CreatedAt,
                    EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(),
                    Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null,
                    NewValues = JsonSerializer.Serialize(rate),
                    UserId = userId
                };

                await _unitOfWork.Rates.AddAsync(rate);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<RateResponse>(rate);
            });
        }

        public async Task DeleteAsync(Guid id, string userId)
        {
            await ExecuteInTransactionAsync(async () =>
            {
                var rate = await _unitOfWork.Rates.GetByIdAsync(id);
                if (rate == null)
                    throw new KeyNotFoundException("Rate not found.");

                if (rate.IsDeleted)
                    throw new BusinessRuleException("Rate is already deleted.");

                rate.IsDeleted = true;
                rate.IsActive = false;
                rate.DeletedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(),
                    Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(rate),
                    NewValues = "Deleted",
                    UserId = userId
                };

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }

        public async Task<RateResponse> UpdateAsync(Guid id, UpdateRateRequest dto, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await GetRateWithDetailsOrThrowAsync(id);
                var oldRate = rate;
                if(string.IsNullOrEmpty(dto.Currency))
                    dto.Currency = rate.Currency;

                if (dto.Price <= 0)
                    dto.Price = rate.Price;

                if (dto.ValidFrom == default)
                    dto.ValidFrom = rate.ValidFrom;

                if (dto.ValidTo == default)
                    dto.ValidTo = rate.ValidTo;

                dto.MaxGrossWeightKg ??= rate.MaxGrossWeightKg;
                dto.MaxNetWeightKg ??= rate.MaxNetWeightKg;
                dto.MaxVolumeCbm ??= rate.MaxVolumeCbm;
                dto.MinTemperatureCelsius ??= rate.MinTemperatureCelsius;
                dto.MaxTemperatureCelsius ??= rate.MaxTemperatureCelsius;

                ValidateRateConstraints(
                dto.MaxGrossWeightKg,
                dto.MaxNetWeightKg,
                dto.MaxVolumeCbm,
                dto.MinTemperatureCelsius,
                dto.MaxTemperatureCelsius);

                if (!RateRules.IsValidDateRange(dto.ValidFrom, dto.ValidTo))
                    throw new BusinessRuleException("Invalid date range.");

                if(!RateRules.IsValidCurrency(dto.Currency))
                    throw new BusinessRuleException($"Invalid currency please choose a valid currency" +
                        $" from the allowed list [{string.Join(", ", RateRules.AllowedCurrencies)}].");
                rate.Price = dto.Price; rate.Currency = dto.Currency; rate.ValidFrom = dto.ValidFrom;
                rate.ValidTo = dto.ValidTo; rate.UpdatedAt = DateTimeOffset.UtcNow;

                var shouldBeActive = RateRules.ShouldBeActive(rate.ValidFrom, rate.ValidTo);
                rate.IsActive = shouldBeActive;

                if (shouldBeActive)
                {
                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId,
                    rate.ContainerTypeId, dto.ValidFrom, dto.ValidTo, rate.Id);
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(),
                    Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldRate),
                    NewValues = JsonSerializer.Serialize(rate),
                    UserId = userId
                };

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();
                return _mapper.Map<RateResponse>(rate);
            });
        }

        public async Task<IEnumerable<RateResponse>> SearchAsync(RateParameters query)
        {
            var rates = await _unitOfWork.Rates.SearchAsync(query);
            return _mapper.Map<IEnumerable<RateResponse>>(rates);
        }

        public async Task<MarketAnalyticsResponse> GetMarketAnalyticsAsync(Guid routeId, Guid containerId, string currency)
        {
            var query = _unitOfWork.Rates
                .GetRatesByRouteAndContainerTypeQuery(routeId, containerId, currency.Trim().ToUpperInvariant());
            
            var analytics = await query
                .GroupBy(x => 1)
                .Select(g => new MarketAnalyticsResponse
                {
                    ActiveCount = g.Count(),
                    AveragePrice = g.Average(x => x.Price),
                    CheapestPrice = g.Min(x => x.Price),
                    HighestPrice = g.Max(x => x.Price),
                    Currency = currency.Trim().ToUpper()
                })
                .FirstOrDefaultAsync();

            if (analytics is null)
                throw new BusinessRuleException("No active rates found for this route, container type, and currency.");

            return analytics;
        }

        public async Task<RateRecommendationResponse> RecommendationAsync(RateRecommendationRequest dto)
        {
            var query = _unitOfWork.Rates.GetRatesByRouteAndContainerTypeQueryForRecommendation
            (dto.RouteId, dto.ContainerTypeId, dto.Currency, dto.MaxPrice);

            query = dto.Priority switch
            {
                RecommendationPriority.Cheapest => query.OrderBy(x => x.Price),

                _ => query.OrderBy(x => x.Price)
            };

            var rates = await query.Take(dto.Limit).ToListAsync();
            if (!rates.Any())
                return new RateRecommendationResponse
                {
                    Recommendations = []
                };

            var cheapestPrice = rates.Min(x => x.Price);
            var averagePrice = rates.Average(x => x.Price);

            var recommendations = rates.Select(rate => new RecommendedRateResponse
            {
                Rate = _mapper.Map<RateResponse>(rate),

                IsCheapest = rate.Price == cheapestPrice,

                Score = rate.Price == cheapestPrice ? 100 : 80,

                RecommendationReason = rate.Price == cheapestPrice
                ? "Cheapest available active rate"
                : "Available valid rate",

                MarketPosition = rate.Price < averagePrice
                ? MarketPosition.BelowMarket
                : rate.Price > averagePrice
                ? MarketPosition.AboveMarket
                : MarketPosition.AverageMarket
            }).ToList();

            return new RateRecommendationResponse
            {
                Recommendations = recommendations
            };
        }

        public async Task<RateResponse?> GetByIdAsync(Guid id)
        {
            var rate = await _unitOfWork.Rates.GetByIdAsync(id, IncludeRateDetails());

            return rate == null ? null : _mapper.Map<RateResponse>(rate);
        }

        public async Task<bool> ChangeRateActive(Guid rateId, string userId)
        {
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await _unitOfWork.Rates.GetByIdAsync(rateId);
                if (rate == null)
                    throw new KeyNotFoundException("Rate not found.");

                var oldRate = rate;

                if (rate.IsDeleted)
                    throw new BusinessRuleException("Cannot change active state of a deleted rate.");

                if (!rate.IsActive)
                {
                    if (!RateRules.CanActivateRate(rate.ValidTo))
                        throw new BusinessRuleException("Cannot activate a rate with an expired validity period.");

                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId,
                    rate.ContainerTypeId, rate.ValidFrom, rate.ValidTo,rate.Id);

                    rate.IsActive = true;
                }
                else
                {
                    rate.IsActive = false;
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow,
                    EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(),
                    Action = nameof(ChangeRateActive).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldRate),
                    NewValues = JsonSerializer.Serialize(rate),
                    UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                _unitOfWork.Rates.Update(rate);

                return rate.IsActive;
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

        private static void ValidateRateConstraints(
        decimal? maxGrossWeightKg,
        decimal? maxNetWeightKg,
        decimal? maxVolumeCbm,
        decimal? minTemperatureCelsius,
        decimal? maxTemperatureCelsius)
        {
            if (maxGrossWeightKg.HasValue && maxGrossWeightKg <= 0)
                throw new BusinessRuleException("Max gross weight must be greater than zero.");

            if (maxNetWeightKg.HasValue && maxNetWeightKg <= 0)
                throw new BusinessRuleException("Max net weight must be greater than zero.");

            if (maxVolumeCbm.HasValue && maxVolumeCbm <= 0)
                throw new BusinessRuleException("Max volume CBM must be greater than zero.");

            if (maxGrossWeightKg.HasValue &&
                maxNetWeightKg.HasValue &&
                maxNetWeightKg > maxGrossWeightKg)
                throw new BusinessRuleException("Max net weight cannot be greater than max gross weight.");

            if (minTemperatureCelsius.HasValue &&
                maxTemperatureCelsius.HasValue &&
                minTemperatureCelsius > maxTemperatureCelsius)
                throw new BusinessRuleException("Minimum temperature cannot be greater than maximum temperature.");
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

        private async Task DeactivateOtherActiveRatesAsync(Guid carrierId, Guid routeId, Guid containerTypeId, DateTimeOffset validFrom,
        DateTimeOffset validTo, Guid? excludeRateId = null)
        {
            var activeRates = await _unitOfWork.Rates
                .GetAvailableRatesByCarrierRouteAndContainerTypeAsync(carrierId, routeId, containerTypeId, validFrom, validTo);

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