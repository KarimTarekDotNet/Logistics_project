using Application.Common;
using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using AutoMapper;
using Domain.Entities.Audits;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Services.Pricing.Quotation
{
    public class QuoteService : IQuoteService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<QuoteService> _logger;

        public QuoteService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager, ILogger<QuoteService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<Result<QuoteResponse>> GetByIdAsync(Guid id, string userId, bool isAdminOrStaff)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null || user.CustomerProfile == null)
                return Result<QuoteResponse>.NotFound("User not found.");

            var quote = await _unitOfWork.Quotes.GetWithItemsAsync(id);
            if (isAdminOrStaff)
            {
                if (quote == null || quote.IsDeleted) return Result<QuoteResponse>.NotFound("Quote not found.");
            }
            else
            {
                if (quote == null || quote.IsDeleted || quote.CustomerId != user.CustomerProfile.Id)
                    return Result<QuoteResponse>.NotFound("Quote not found.");
            }
            return Result<QuoteResponse>.Success(_mapper.Map<QuoteResponse>(quote));
        }

        public async Task<Result<IEnumerable<QuoteResponse>>> GetAllAsync(QueryParameters query)
        {
            var quotes = await _unitOfWork.Quotes.GetAllWithDetailsAsync(query);
            return Result<IEnumerable<QuoteResponse>>.Success(_mapper.Map<IEnumerable<QuoteResponse>>(quotes));
        }

        public async Task<Result<IEnumerable<QuoteResponse>>> GetByCustomerNameAsync(string customerName, QueryParameters query)
        {
            var quotes = await _unitOfWork.Quotes.GetByCustomerNameAsync(customerName, query);
            return Result<IEnumerable<QuoteResponse>>.Success(_mapper.Map<IEnumerable<QuoteResponse>>(quotes.Where(q => !q.IsDeleted)));
        }

        public async Task<Result<IEnumerable<QuoteResponse>>> GetByRouteIdAsync(Guid routeId, QueryParameters query)
        {
            var quotes = await _unitOfWork.Quotes.GetByRouteAsync(routeId, query);
            return Result<IEnumerable<QuoteResponse>>.Success(_mapper.Map<IEnumerable<QuoteResponse>>(quotes.Where(q => !q.IsDeleted)));
        }

        public async Task<Result<IEnumerable<QuoteResponse>>> GetMyQuotesAsync(string userId, QueryParameters query)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null || user.CustomerProfile == null)
                return Result<IEnumerable<QuoteResponse>>.Success(Enumerable.Empty<QuoteResponse>());

            var quotes = await _unitOfWork.Quotes.GetByCustomerIdAsync(user.CustomerProfile.Id, query);
            return Result<IEnumerable<QuoteResponse>>.Success(_mapper.Map<IEnumerable<QuoteResponse>>(quotes));
        }

        public async Task<Result<QuoteResponse>> CreateAsync(CreateQuoteRequest dto, string userId)
        {
            _logger.LogInformation("Creating quote for customer {CustomerId} by user {UserId}", dto.CustomerId, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                var customer = await _unitOfWork.Customers.GetDetailsByIdAsync(dto.CustomerId);
                if (customer == null)
                    return Result<QuoteResponse>.NotFound("Customer not found.");

                var rate = await _unitOfWork.Rates.GetById(dto.RateId);
                if (rate == null || rate.IsDeleted)
                    return Result<QuoteResponse>.NotFound("Rate not found.");

                var now = DateTimeOffset.UtcNow;
                if (!rate.IsActive)
                    return Result<QuoteResponse>.Failure("Rate is not active.");
                if (rate.ValidFrom > now || rate.ValidTo < now)
                    return Result<QuoteResponse>.Failure("Rate is not valid at the current time.");

                var cargoCheck = ValidateCargoAgainstRate(dto, rate);
                if (cargoCheck != null) return Result<QuoteResponse>.Failure(cargoCheck);

                var quote = new Quote
                {
                    CustomerId = customer.Id, RouteId = rate.RouteId, ContainerTypeId = rate.ContainerTypeId,
                    FinalPrice = rate.Price, Currency = rate.Currency, CreatedAt = now,
                    CarrierId = rate.CarrierId, RateId = rate.Id,
                    RequestedGrossWeightKg = dto.RequestedGrossWeightKg, RequestedNetWeightKg = dto.RequestedNetWeightKg,
                    RequestedVolumeCbm = dto.RequestedVolumeCbm, IsHazardous = dto.IsHazardous,
                    RequiredTemperatureCelsius = dto.RequiredTemperatureCelsius,
                    RequestedChargeableWeightKg = ShipmentWeightCalculator.CalculateItemChargeableWeight(dto.RequestedGrossWeightKg, dto.RequestedVolumeCbm),
                    Status = QuoteStatus.Pending
                };

                var audit = new AuditLog
                {
                    CreatedAt = quote.CreatedAt, EntityId = quote.Id,
                    EntityName = nameof(Quote).ToUpper(), Action = nameof(CreateAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = null, NewValues = JsonSerializer.Serialize(quote), UserId = userId
                };

                await _unitOfWork.Quotes.AddAsync(quote);
                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                var created = await _unitOfWork.Quotes.GetWithItemsAsync(quote.Id);
                _logger.LogInformation("Quote {Id} created successfully", quote.Id);
                return Result<QuoteResponse>.Success(_mapper.Map<QuoteResponse>(created), 201);
            });
        }

        public async Task<Result<QuoteResponse>> AcceptFromUserAsync(Guid id, string userId)
        {
            _logger.LogInformation("User {UserId} accepting quote {QuoteId}", userId, id);
            return await ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) return Result<QuoteResponse>.Unauthorized("User is not authenticated.");
                if (user.CustomerProfile == null) return Result<QuoteResponse>.Failure("User does not have a customer profile.");

                var quote = await _unitOfWork.Quotes.GetByIdAndCustomerIdAsync(id, user.CustomerProfile.Id);
                if (quote == null) return Result<QuoteResponse>.NotFound("Quote was not found or does not belong to this customer.");
                if (quote.Status != QuoteStatus.Pending)
                    return Result<QuoteResponse>.Failure($"Only quotes pending customer approval can be accepted. Current status: {quote.Status}.");

                var oldQuote = quote;
                quote.Status = QuoteStatus.Accepted;
                quote.UpdatedAt = DateTimeOffset.UtcNow;

                var shipment = new Shipment
                {
                    QuoteId = quote.Id, Quote = quote, CustomerId = quote.CustomerId,
                    RouteId = quote.RouteId, ContainerTypeId = quote.ContainerTypeId, CarrierId = quote.CarrierId,
                    AgreedPrice = quote.FinalPrice, Currency = quote.Currency,
                    Status = ShipmentStatus.Created, CreatedAt = DateTimeOffset.UtcNow,
                    AllowedGrossWeightKg = quote.RequestedGrossWeightKg, AllowedNetWeightKg = quote.RequestedNetWeightKg,
                    AllowedVolumeCbm = quote.RequestedVolumeCbm, IsHazardousAllowed = quote.IsHazardous,
                    AllowedChargeableWeightKg = quote.RequestedChargeableWeightKg
                };

                await _unitOfWork.Shipments.AddAsync(shipment);

                var invoice = new Invoice
                {
                    ShipmentId = shipment.Id, Shipment = shipment,
                    InvoiceNumber = InvoiceHelper.GenerateInvoiceNumber(shipment.Customer.NationalId!),
                    Currency = InvoiceHelper.NormalizeAndValidateCurrency(shipment.Currency),
                    NetShipmentPrice = shipment.AgreedPrice, SubTotal = shipment.AgreedPrice,
                    TaxAmount = 0.14m * shipment.AgreedPrice,
                    TotalAmount = shipment.AgreedPrice + (0.14m * shipment.AgreedPrice),
                    PaymentStatus = PaymentStatus.Pending, IssuedAt = DateTimeOffset.UtcNow,
                    DueDate = DateTimeOffset.UtcNow.AddDays(7), CreatedAt = DateTimeOffset.UtcNow,
                    PayerType = PayerType.Shipper,
                };

                var charge = new ShipmentCharge
                {
                    Shipment = shipment, InvoiceId = invoice.Id, Invoice = invoice,
                    ChargeType = ChargeType.OceanFreight, PayerType = invoice.PayerType,
                    Description = "Ocean freight charge based on approved quote Acceptance",
                    Amount = quote.FinalPrice, TaxAmount = invoice.TaxAmount,
                    Currency = quote.Currency, CreatedAt = DateTimeOffset.UtcNow,
                };

                invoice.Charges.Add(charge);
                await _unitOfWork.ShipmentCharges.AddAsync(charge);
                await _unitOfWork.Invoices.AddAsync(invoice);

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = quote.Id,
                    EntityName = nameof(Quote).ToUpper(), Action = nameof(AcceptFromUserAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldQuote), NewValues = JsonSerializer.Serialize(quote), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Quote {Id} accepted by user {UserId}", id, userId);
                return Result<QuoteResponse>.Success(_mapper.Map<QuoteResponse>(quote));
            });
        }

        public async Task<Result<QuoteResponse>> RejectFromUserAsync(Guid id, string userId, string reason)
        {
            _logger.LogInformation("User {UserId} rejecting quote {QuoteId}", userId, id);
            return await ExecuteInTransactionAsync(async () =>
            {
                var user = await _userManager.Users.Include(x => x.CustomerProfile).FirstOrDefaultAsync(x => x.Id == userId);
                if (user == null) return Result<QuoteResponse>.Unauthorized("User is not authenticated.");
                if (user.CustomerProfile == null) return Result<QuoteResponse>.Failure("User does not have a customer profile.");

                var quote = await _unitOfWork.Quotes.GetByIdAndCustomerIdAsync(id, user.CustomerProfile.Id);
                if (quote == null) return Result<QuoteResponse>.NotFound("Quote was not found or does not belong to this customer.");
                if (quote.Status != QuoteStatus.Pending)
                    return Result<QuoteResponse>.Failure($"Only quotes pending customer approval can be accepted. Current status: {quote.Status}.");

                var oldQuote = quote;
                quote.Status = QuoteStatus.Rejected;
                quote.UpdatedAt = DateTimeOffset.UtcNow;
                quote.Reason = reason;

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = quote.Id,
                    EntityName = nameof(Quote).ToUpper(), Action = nameof(RejectFromUserAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(oldQuote), NewValues = JsonSerializer.Serialize(quote), UserId = userId
                };

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Quote {Id} rejected by user {UserId}", id, userId);
                return Result<QuoteResponse>.Success(_mapper.Map<QuoteResponse>(quote));
            });
        }

        public async Task<Result> DeleteAsync(Guid id, bool isAdmin, string userId)
        {
            _logger.LogInformation("Deleting quote {Id} by user {UserId}", id, userId);
            return await ExecuteInTransactionAsync(async () =>
            {
                if (!isAdmin)
                    return Result.Forbidden("Only staff or administrators can review quote requests.");

                var quote = await _unitOfWork.Quotes.GetWithItemsAsync(id);
                if (quote == null || quote.IsDeleted)
                {
                    _logger.LogWarning("Quote {Id} not found for deletion", id);
                    return Result.NotFound("Quote not found.");
                }

                var audit = new AuditLog
                {
                    CreatedAt = DateTimeOffset.UtcNow, EntityId = quote.Id,
                    EntityName = nameof(Quote).ToUpper(), Action = nameof(DeleteAsync).ToUpper(),
                    IpAddress = await IpAddressHelper.GetRealPublicIpAsync(),
                    OldValues = JsonSerializer.Serialize(quote), NewValues = "Deleted", UserId = userId
                };

                quote.IsDeleted = true;
                quote.DeletedAt = DateTimeOffset.UtcNow;
                quote.UpdatedAt = DateTimeOffset.UtcNow;

                await _unitOfWork.AuditLog.Add(audit);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Quote {Id} deleted", id);
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(QuoteService));
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
                _logger.LogError(ex, "Transaction failed in {Service}", nameof(QuoteService));
                throw;
            }
        }

        private static string? ValidateCargoAgainstRate(CreateQuoteRequest dto, Rate rate)
        {
            if (dto.RequestedGrossWeightKg <= 0) return "Gross weight must be greater than zero.";
            if (dto.RequestedNetWeightKg <= 0) return "Net weight must be greater than zero.";
            if (dto.RequestedVolumeCbm <= 0) return "Volume CBM must be greater than zero.";
            if (dto.RequestedNetWeightKg > dto.RequestedGrossWeightKg) return "Net weight cannot be greater than gross weight.";
            if (rate.MaxGrossWeightKg.HasValue && dto.RequestedGrossWeightKg > rate.MaxGrossWeightKg.Value)
                return $"Gross weight exceeds the allowed limit for this rate. Max allowed: {rate.MaxGrossWeightKg.Value} kg.";
            if (rate.MaxNetWeightKg.HasValue && dto.RequestedNetWeightKg > rate.MaxNetWeightKg.Value)
                return $"Net weight exceeds the allowed limit for this rate. Max allowed: {rate.MaxNetWeightKg.Value} kg.";
            if (rate.MaxVolumeCbm.HasValue && dto.RequestedVolumeCbm > rate.MaxVolumeCbm.Value)
                return $"Volume exceeds the allowed limit for this rate. Max allowed: {rate.MaxVolumeCbm.Value} CBM.";
            if (dto.IsHazardous && !rate.AllowsHazardous) return "Hazardous cargo is not allowed for this rate.";
            if (dto.RequiredTemperatureCelsius.HasValue)
            {
                if (!rate.MinTemperatureCelsius.HasValue || !rate.MaxTemperatureCelsius.HasValue)
                    return "Temperature-controlled cargo is not supported for this rate.";
                if (dto.RequiredTemperatureCelsius.Value < rate.MinTemperatureCelsius.Value || dto.RequiredTemperatureCelsius.Value > rate.MaxTemperatureCelsius.Value)
                    return $"Required temperature is outside the allowed range for this rate. Allowed range: {rate.MinTemperatureCelsius.Value} to {rate.MaxTemperatureCelsius.Value} °C.";
            }
            return null;
        }
    }
}
