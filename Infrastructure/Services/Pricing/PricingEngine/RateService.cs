using Application.ApplicationRules;
using Application.Common;
using Application.DTOs.Pricing.PricingEngine.Rates;
using Application.DTOs.Pricing.Recommendations;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Models;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Pricing.PricingEngine
{
    public class RateService : IRateService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RateService> _logger;

        public RateService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<RateService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<int>> CountAsync()
        {
            var count = await _unitOfWork.Rates.CountAsync();
            return Result<int>.Success(count >= 0 ? count.Value : 0);
        }

        public async Task<Result<RateResponse>> CreateAsync(CreateRateRequest dto, string userId)
        {
            _logger.LogInformation("Creating rate for carrier {CarrierId} by user {UserId}", dto.CarrierId, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var refCheck = await EnsureReferencesExistAsync(dto.CarrierId, dto.RouteId, dto.ContainerTypeId);
                if (refCheck != null) return Result<RateResponse>.NotFound(refCheck);

                var constraintCheck = ValidateRateConstraints(dto.MaxGrossWeightKg, dto.MaxNetWeightKg, dto.MaxVolumeCbm, dto.MinTemperatureCelsius, dto.MaxTemperatureCelsius);
                if (constraintCheck != null) return Result<RateResponse>.Failure(constraintCheck);

                var rate = _mapper.Map<Rate>(dto);
                rate.IsActive = RateRules.ShouldBeActive(rate.ValidFrom, rate.ValidTo);
                rate.CreatedAt = DateTimeOffset.UtcNow;

                var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(dto.ContainerTypeId);
                if (containerType == null)
                    return Result<RateResponse>.NotFound("Container type not found.");

                if (dto.AllowsHazardous == true && !containerType.Name.Contains("Reefer", StringComparison.OrdinalIgnoreCase))
                    return Result<RateResponse>.Failure("Temperature-controlled cargo requires a reefer container.");

                if (rate.IsActive)
                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId, rate.ContainerTypeId, dto.ValidFrom, dto.ValidTo);

                var audit = new AuditLog
                {
                    CreatedAt = rate.CreatedAt, EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(), Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null, NewValues = JsonSerializer.Serialize(rate), UserId = userId
                };

                await _unitOfWork.Rates.AddAsync(rate);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Rate {Id} created successfully", rate.Id);
                return Result<RateResponse>.Success(_mapper.Map<RateResponse>(rate), 201);
            });
        }

        public async Task<Result> DeleteAsync(Guid id, string userId)
        {
            _logger.LogInformation("Deleting rate {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await _unitOfWork.Rates.GetByIdAsync(id);
                if (rate == null)
                {
                    _logger.LogWarning("Rate {Id} not found for deletion", id);
                    return Result.NotFound("Rate not found.");
                }
                if (rate.IsDeleted)
                    return Result.Failure("Rate is already deleted.");

                rate.IsDeleted = true;
                rate.IsActive = false;
                rate.DeletedAt = DateTimeOffset.UtcNow;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(rate), NewValues = "Deleted", UserId = userId
                };

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Rate {Id} deleted", id);
                return Result.Success();
            });
        }

        public async Task<Result<RateResponse>> UpdateAsync(Guid id, UpdateRateRequest dto, string userId)
        {
            _logger.LogInformation("Updating rate {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await GetRateWithDetailsAsync(id);
                if (rate == null)
                {
                    _logger.LogWarning("Rate {Id} not found for update", id);
                    return Result<RateResponse>.NotFound("Rate not found.");
                }

                var oldRate = rate;
                if (string.IsNullOrEmpty(dto.Currency)) dto.Currency = rate.Currency;
                if (dto.Price <= 0) dto.Price = rate.Price;
                if (dto.ValidFrom == default) dto.ValidFrom = rate.ValidFrom;
                if (dto.ValidTo == default) dto.ValidTo = rate.ValidTo;
                dto.MaxGrossWeightKg ??= rate.MaxGrossWeightKg;
                dto.MaxNetWeightKg ??= rate.MaxNetWeightKg;
                dto.MaxVolumeCbm ??= rate.MaxVolumeCbm;
                dto.MinTemperatureCelsius ??= rate.MinTemperatureCelsius;
                dto.MaxTemperatureCelsius ??= rate.MaxTemperatureCelsius;

                var constraintCheck = ValidateRateConstraints(dto.MaxGrossWeightKg, dto.MaxNetWeightKg, dto.MaxVolumeCbm, dto.MinTemperatureCelsius, dto.MaxTemperatureCelsius);
                if (constraintCheck != null) return Result<RateResponse>.Failure(constraintCheck);

                if (!RateRules.IsValidDateRange(dto.ValidFrom, dto.ValidTo))
                    return Result<RateResponse>.Failure("Invalid date range.");

                if (!RateRules.IsValidCurrency(dto.Currency))
                    return Result<RateResponse>.Failure($"Invalid currency please choose a valid currency from the allowed list [{string.Join(", ", RateRules.AllowedCurrencies)}].");

                rate.Price = dto.Price; rate.Currency = dto.Currency;
                rate.ValidFrom = dto.ValidFrom; rate.ValidTo = dto.ValidTo;
                rate.UpdatedAt = DateTimeOffset.UtcNow;

                var shouldBeActive = RateRules.ShouldBeActive(rate.ValidFrom, rate.ValidTo);
                rate.IsActive = shouldBeActive;

                if (shouldBeActive)
                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId, rate.ContainerTypeId, dto.ValidFrom, dto.ValidTo, rate.Id);

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(), Action = nameof(UpdateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldRate), NewValues = JsonSerializer.Serialize(rate), UserId = userId
                };

                _unitOfWork.Rates.Update(rate);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Rate {Id} updated successfully", id);
                return Result<RateResponse>.Success(_mapper.Map<RateResponse>(rate));
            });
        }

        public async Task<Result<IEnumerable<RateResponse>>> SearchAsync(RateParameters query)
        {
            var rates = await _unitOfWork.Rates.SearchAsync(query);
            return Result<IEnumerable<RateResponse>>.Success(_mapper.Map<IEnumerable<RateResponse>>(rates));
        }

        public async Task<Result<MarketAnalyticsResponse>> GetMarketAnalyticsAsync(Guid routeId, Guid containerId, string currency)
        {
            var query = _unitOfWork.Rates.GetRatesByRouteAndContainerTypeQuery(routeId, containerId, currency.Trim().ToUpperInvariant());

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
            {
                _logger.LogWarning("No active rates found for route {RouteId}, container {ContainerId}, currency {Currency}", routeId, containerId, currency);
                return Result<MarketAnalyticsResponse>.NotFound("No active rates found for this route, container type, and currency.");
            }

            return Result<MarketAnalyticsResponse>.Success(analytics);
        }

        public async Task<Result<RateRecommendationResponse>> RecommendationAsync(RateRecommendationRequest dto)
        {
            var query = _unitOfWork.Rates.GetRatesByRouteAndContainerTypeQueryForRecommendation(dto.RouteId, dto.ContainerTypeId, dto.Currency, dto.MaxPrice);

            query = dto.Priority switch
            {
                RecommendationPriority.Cheapest => query.OrderBy(x => x.Price),
                _ => query.OrderBy(x => x.Price)
            };

            var rates = await query.Take(dto.Limit).ToListAsync();
            if (!rates.Any())
                return Result<RateRecommendationResponse>.Success(new RateRecommendationResponse { Recommendations = [] });

            var cheapestPrice = rates.Min(x => x.Price);
            var averagePrice = rates.Average(x => x.Price);

            var recommendations = rates.Select(rate => new RecommendedRateResponse
            {
                Rate = _mapper.Map<RateResponse>(rate),
                IsCheapest = rate.Price == cheapestPrice,
                Score = rate.Price == cheapestPrice ? 100 : 80,
                RecommendationReason = rate.Price == cheapestPrice ? "Cheapest available active rate" : "Available valid rate",
                MarketPosition = rate.Price < averagePrice ? MarketPosition.BelowMarket : rate.Price > averagePrice ? MarketPosition.AboveMarket : MarketPosition.AverageMarket
            }).ToList();

            return Result<RateRecommendationResponse>.Success(new RateRecommendationResponse { Recommendations = recommendations });
        }

        public async Task<Result<RateResponse>> GetByIdAsync(Guid id)
        {
            var rate = await _unitOfWork.Rates.GetByIdAsync(id, IncludeRateDetails());
            if (rate == null)
            {
                _logger.LogWarning("Rate {Id} not found", id);
                return Result<RateResponse>.NotFound("Rate not found.");
            }
            return Result<RateResponse>.Success(_mapper.Map<RateResponse>(rate));
        }

        public async Task<Result<bool>> ChangeRateActive(Guid rateId, string userId)
        {
            _logger.LogInformation("Toggling active state for rate {RateId} by user {UserId}", rateId, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var rate = await _unitOfWork.Rates.GetByIdAsync(rateId);
                if (rate == null)
                {
                    _logger.LogWarning("Rate {Id} not found for toggle", rateId);
                    return Result<bool>.NotFound("Rate not found.");
                }

                var oldRate = rate;
                if (rate.IsDeleted)
                    return Result<bool>.Failure("Cannot change active state of a deleted rate.");

                if (!rate.IsActive)
                {
                    if (!RateRules.CanActivateRate(rate.ValidTo))
                        return Result<bool>.Failure("Cannot activate a rate with an expired validity period.");

                    await DeactivateOtherActiveRatesAsync(rate.CarrierId, rate.RouteId, rate.ContainerTypeId, rate.ValidFrom, rate.ValidTo, rate.Id);
                    rate.IsActive = true;
                }
                else
                {
                    rate.IsActive = false;
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = rate.Id,
                    EntityName = nameof(Rate).ToUpper(), Action = nameof(ChangeRateActive).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldRate), NewValues = JsonSerializer.Serialize(rate), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                _unitOfWork.Rates.Update(rate);

                _logger.LogInformation("Rate {Id} active state changed to {IsActive}", rateId, rate.IsActive);
                return Result<bool>.Success(rate.IsActive);
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(RateService));
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(RateService));
                throw;
            }
        }

        private static string? ValidateRateConstraints(decimal? maxGrossWeightKg, decimal? maxNetWeightKg, decimal? maxVolumeCbm, decimal? minTemperatureCelsius, decimal? maxTemperatureCelsius)
        {
            if (maxGrossWeightKg.HasValue && maxGrossWeightKg <= 0) return "Max gross weight must be greater than zero.";
            if (maxNetWeightKg.HasValue && maxNetWeightKg <= 0) return "Max net weight must be greater than zero.";
            if (maxVolumeCbm.HasValue && maxVolumeCbm <= 0) return "Max volume CBM must be greater than zero.";
            if (maxGrossWeightKg.HasValue && maxNetWeightKg.HasValue && maxNetWeightKg > maxGrossWeightKg)
                return "Max net weight cannot be greater than max gross weight.";
            if (minTemperatureCelsius.HasValue && maxTemperatureCelsius.HasValue && minTemperatureCelsius > maxTemperatureCelsius)
                return "Minimum temperature cannot be greater than maximum temperature.";
            return null;
        }

        private async Task<string?> EnsureReferencesExistAsync(Guid carrierId, Guid routeId, Guid containerTypeId)
        {
            if (await _unitOfWork.Carriers.GetByIdAsync(carrierId) == null) return "Carrier not found.";
            if (await _unitOfWork.Routes.GetByIdAsync(routeId) == null) return "Route not found.";
            if (await _unitOfWork.ContainerTypes.GetByIdAsync(containerTypeId) == null) return "Container type not found.";
            return null;
        }

        private async Task<Rate?> GetRateWithDetailsAsync(Guid rateId)
        {
            return await _unitOfWork.Rates.GetByIdAsync(rateId, IncludeRateDetails());
        }

        private static Func<IQueryable<Rate>, IQueryable<Rate>> IncludeRateDetails()
        {
            return query => query
                .Include(r => r.Carrier)
                .Include(r => r.Route).ThenInclude(r => r.FromPort)
                .Include(r => r.Route).ThenInclude(r => r.ToPort)
                .Include(r => r.ContainerType);
        }

        private async Task DeactivateOtherActiveRatesAsync(Guid carrierId, Guid routeId, Guid containerTypeId, DateTimeOffset validFrom, DateTimeOffset validTo, Guid? excludeRateId = null)
        {
            var activeRates = await _unitOfWork.Rates.GetAvailableRatesByCarrierRouteAndContainerTypeAsync(carrierId, routeId, containerTypeId, validFrom, validTo);
            foreach (var activeRate in activeRates)
            {
                if (excludeRateId.HasValue && activeRate.Id == excludeRateId.Value) continue;
                activeRate.IsActive = false;
                _unitOfWork.Rates.Update(activeRate);
            }
        }
    }
}
