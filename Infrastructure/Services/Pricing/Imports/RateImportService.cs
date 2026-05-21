using Application.ApplicationRules;
using Application.DTOs.Pricing.Imports;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.Imports;
using Domain.Entities.Pricing.Imports;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.ShippingCore;
using Domain.Enums;
using Domain.Exceptions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Pricing.Imports
{
    public class RateImportService : IRateImportService
    {
        private readonly IUnitOfWork _unitOfWork;

        public RateImportService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ImportRatesResponse> ImportAsync(ImportRatesRequest request, IntegrationRequestContext context,
        CancellationToken cancellationToken = default)
        {
            var response = new ImportRatesResponse
            {
                TotalReceived = request.Rates.Count
            };

            foreach (var item in request.Rates)
            {
                try
                {
                    var result = await ExecuteInTransactionAsync(async () =>
                    {
                        var integrationMessage = new IntegrationMessage
                        {
                            ExternalMessageId = item.ExternalMessageId,
                            Source = context.Source,
                            ProcessingStatus = Status.Processing,
                            CreatedAt = DateTimeOffset.UtcNow
                        };

                        await _unitOfWork.IntegrationMessage.AddAsync(integrationMessage);

                        await _unitOfWork.SaveChangesAsync();

                        var references = await ResolveReferencesAsync(item);

                        var existingRate = await GetExistingRateAsync(
                            references.Carrier.Id,
                            references.Route.Id,
                            references.ContainerType.Id);

                        ImportRateItemResult result;

                        if (existingRate == null)
                        {
                            var newRate = await CreateRateAsync(item, references);

                            result = new ImportRateItemResult
                            {
                                ExternalMessageId = item.ExternalMessageId,
                                Status = "Imported",
                                Message = "Rate created successfully.",
                                RateId = newRate.Id
                            };
                        }
                        else if (!HasChanges(existingRate, item))
                        {
                            result = new ImportRateItemResult
                            {
                                ExternalMessageId = item.ExternalMessageId,
                                Status = "Skipped",
                                Message = "No changes detected.",
                                RateId = existingRate.Id
                            };
                        }
                        else
                        {
                            await UpdateRateAsync(existingRate, item);

                            result = new ImportRateItemResult
                            {
                                ExternalMessageId = item.ExternalMessageId,
                                Status = "Updated",
                                Message = "Rate updated successfully.",
                                RateId = existingRate.Id
                            };
                        }

                        integrationMessage.ProcessingStatus = Status.Processed;
                        integrationMessage.ProcessedAt = DateTimeOffset.UtcNow;
                        return result;
                    });

                    response.Results.Add(result);
                    switch (result.Status)
                    {
                        case "Imported":
                            response.Imported++;
                            break;
                        case "Updated":
                            response.Updated++;
                            break;
                        case "Skipped":
                            response.Skipped++;
                            break;
                    }
                }
                catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
                {
                    response.Skipped++;
                    response.Results.Add(new ImportRateItemResult
                    {
                        ExternalMessageId = item.ExternalMessageId,
                        Status = "Skipped",
                        Message = "Duplicate integration message."
                    });
                }
                catch (Exception ex)
                {
                    response.Failed++;

                    response.Results.Add(new ImportRateItemResult
                    {
                        ExternalMessageId = item.ExternalMessageId,
                        Status = "Failed",
                        Message = ex.Message
                    });

                    await ExecuteInTransactionAsync(async () =>
                    {
                        var integrationMessage = await _unitOfWork.IntegrationMessage
                        .GetByExternalMessageIdAndSourceAsync(item.ExternalMessageId, context.Source);

                        if (integrationMessage != null)
                        {
                            integrationMessage.ProcessingStatus = Status.Failed;
                            integrationMessage.FailedAt = DateTimeOffset.UtcNow;
                            integrationMessage.ErrorMessage = ex.Message;
                        }

                        return true;
                    });
                }
            }

            return response;
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

        private static bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            return ex.InnerException is SqlException sqlException
                   && (sqlException.Number == 2601 || sqlException.Number == 2627);
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

        private async Task<ImportReferences> ResolveReferencesAsync(ImportRateItemRequest item)
        {
            var carrier = await _unitOfWork.Carriers.GetByNameOrCodeAsync(item.CarrierName.Trim());
            if (carrier == null)
                throw new KeyNotFoundException($"Carrier '{item.CarrierName}' not found.");

            var fromPort = await _unitOfWork.Ports.GetByNameOrCodeAsync(item.FromPortCode.Trim());
            if (fromPort == null)
                throw new KeyNotFoundException($"From port '{item.FromPortCode}' not found.");

            var toPort = await _unitOfWork.Ports.GetByNameOrCodeAsync(item.ToPortCode.Trim());
            if (toPort == null)
                throw new KeyNotFoundException($"To port '{item.ToPortCode}' not found.");

            var containerType = await _unitOfWork.ContainerTypes.GetByNameAsync(item.ContainerTypeName.Trim());
            if (containerType == null)
                throw new KeyNotFoundException($"Container type '{item.ContainerTypeName}' not found.");

            var route = await _unitOfWork.Routes.GetByPortsAsync(fromPort.Id, toPort.Id);
            if (route == null)
                throw new KeyNotFoundException("Matching route not found.");

            return new ImportReferences
            {
                Carrier = carrier,
                Route = route,
                ContainerType = containerType
            };
        }

        private async Task<Rate?> GetExistingRateAsync(Guid carrierId, Guid routeId, Guid containerTypeId)
        {
            var existingRates = await _unitOfWork.Rates
                .GetByCarrierRouteAndContainerTypeAsync(carrierId, routeId, containerTypeId);

            return existingRates
                .Where(r => !r.IsDeleted)
                .OrderByDescending(r => r.CreatedAt)
                .FirstOrDefault();
        }

        private async Task<Rate> CreateRateAsync(ImportRateItemRequest item, ImportReferences references)
        {
            ValidateRateConstraints(
                item.MaxGrossWeightKg,
                item.MaxNetWeightKg,
                item.MaxVolumeCbm,
                item.MinTemperatureCelsius,
                item.MaxTemperatureCelsius);

            var newRate = new Rate
            {
                CarrierId = references.Carrier.Id,
                RouteId = references.Route.Id,
                ContainerTypeId = references.ContainerType.Id,
                Price = item.Price,
                Currency = item.Currency.Trim().ToUpper(),
                ValidFrom = item.ValidFrom,
                ValidTo = item.ValidTo,
                IsActive = RateRules.ShouldBeActive(item.ValidFrom, item.ValidTo),
                CreatedAt = DateTimeOffset.UtcNow,
                AllowsHazardous = item.AllowsHazardous,
                MaxGrossWeightKg = item.MaxGrossWeightKg,
                MaxNetWeightKg = item.MaxNetWeightKg,
                MaxTemperatureCelsius = item.MaxTemperatureCelsius,
                MaxVolumeCbm = item.MaxVolumeCbm,
                MinTemperatureCelsius = item.MinTemperatureCelsius
            };

            if (newRate.IsActive)
            {
                await DeactivateOtherActiveRatesAsync(
                    references.Carrier.Id,
                    references.Route.Id,
                    references.ContainerType.Id,
                    newRate.ValidFrom,
                    newRate.ValidTo);
            }

            await _unitOfWork.Rates.AddAsync(newRate);

            return newRate;
        }

        private async Task UpdateRateAsync(Rate existingRate, ImportRateItemRequest item)
        {
            ValidateRateConstraints(
            item.MaxGrossWeightKg,
            item.MaxNetWeightKg,
            item.MaxVolumeCbm,
            item.MinTemperatureCelsius,
            item.MaxTemperatureCelsius);

            existingRate.MaxGrossWeightKg = item.MaxGrossWeightKg;
            existingRate.MaxNetWeightKg = item.MaxNetWeightKg;
            existingRate.MaxVolumeCbm = item.MaxVolumeCbm;
            existingRate.AllowsHazardous = item.AllowsHazardous;
            existingRate.MinTemperatureCelsius = item.MinTemperatureCelsius;
            existingRate.MaxTemperatureCelsius = item.MaxTemperatureCelsius;

            existingRate.Price = item.Price;
            existingRate.Currency = item.Currency.Trim().ToUpper();
            existingRate.ValidFrom = item.ValidFrom;
            existingRate.ValidTo = item.ValidTo;
            existingRate.IsActive = RateRules.ShouldBeActive(existingRate.ValidFrom, existingRate.ValidTo);
            existingRate.UpdatedAt = DateTimeOffset.UtcNow;

            if (existingRate.IsActive)
            {
                await DeactivateOtherActiveRatesAsync(
                    existingRate.CarrierId,
                    existingRate.RouteId,
                    existingRate.ContainerTypeId,
                    existingRate.ValidFrom,
                    existingRate.ValidTo,
                    existingRate.Id);
            }

            _unitOfWork.Rates.Update(existingRate);
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

        private static bool HasChanges(Rate existingRate, ImportRateItemRequest item)
        {
            return existingRate.Price != item.Price
                || existingRate.Currency != item.Currency.Trim().ToUpper()
                || existingRate.ValidFrom != item.ValidFrom
                || existingRate.ValidTo != item.ValidTo
                || existingRate.MaxGrossWeightKg != item.MaxGrossWeightKg
                || existingRate.MaxNetWeightKg != item.MaxNetWeightKg
                || existingRate.MaxVolumeCbm != item.MaxVolumeCbm
                || existingRate.AllowsHazardous != item.AllowsHazardous
                || existingRate.MinTemperatureCelsius != item.MinTemperatureCelsius
                || existingRate.MaxTemperatureCelsius != item.MaxTemperatureCelsius;
        }

        private sealed class ImportReferences
        {
            public required Carrier Carrier { get; init; }
            public required Route Route { get; init; }
            public required ContainerType ContainerType { get; init; }
        }
    }
}