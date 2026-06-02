using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using AutoMapper;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Exceptions;
using Infrastructure.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Pricing.Quotation
{
    public class QuoteService : IQuoteService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public QuoteService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<QuoteResponse?> GetByIdAsync(Guid id, string userId, bool isAdminOrStaff)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null || user.CustomerProfile == null)
                throw new KeyNotFoundException("User not found.");

            var quote = await _unitOfWork.Quotes.GetWithItemsAsync(id);
            if (isAdminOrStaff)
            { 
                if (quote == null || quote.IsDeleted)
                    return null;
            }
            else
            {
                if (quote == null || quote.IsDeleted || quote.CustomerId != user.CustomerProfile.Id)
                    return null;
            }
            return _mapper.Map<QuoteResponse>(quote);
        }

        public async Task<IEnumerable<QuoteResponse>> GetAllAsync(QueryParameters query)
        {
            var quotes = await _unitOfWork.Quotes.GetAllWithDetailsAsync(query);

            return _mapper.Map<IEnumerable<QuoteResponse>>(quotes);
        }

        public async Task<IEnumerable<QuoteResponse>> GetByCustomerNameAsync(string customerName, QueryParameters query)
        {
            var quotes = await _unitOfWork.Quotes.GetByCustomerNameAsync(customerName, query);

            return _mapper.Map<IEnumerable<QuoteResponse>>(quotes.Where(q => !q.IsDeleted));
        }

        public async Task<IEnumerable<QuoteResponse>> GetByRouteIdAsync(Guid routeId, QueryParameters query)
        {
            var quotes = await _unitOfWork.Quotes.GetByRouteAsync(routeId, query);
            return _mapper.Map<IEnumerable<QuoteResponse>>(quotes.Where(q => !q.IsDeleted));
        }

        public async Task<IEnumerable<QuoteResponse>> GetMyQuotesAsync(string userId, QueryParameters query)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                return Enumerable.Empty<QuoteResponse>();

            var quotes = await _unitOfWork.Quotes.GetByCustomerIdAsync(user.CustomerProfile.Id, query);
            return _mapper.Map<IEnumerable<QuoteResponse>>(quotes);
        }

        public async Task<QuoteResponse> CreateAsync(CreateQuoteRequest dto)
        {
            var customer = await _unitOfWork.Customers.GetDetailsByIdAsync(dto.CustomerId);
            if(customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var rate = await _unitOfWork.Rates.GetById(dto.RateId);
            if (rate == null || rate.IsDeleted)
                throw new KeyNotFoundException("Rate not found.");

            var now = DateTimeOffset.UtcNow;

            if (!rate.IsActive)
                throw new BusinessRuleException("Rate is not active.");

            if (rate.ValidFrom > now || rate.ValidTo < now)
                throw new BusinessRuleException("Rate is not valid at the current time.");

            ValidateCargoAgainstRate(dto, rate);

            var quote = new Quote
            {
                CustomerId = customer.Id,
                RouteId = rate.RouteId,
                ContainerTypeId = rate.ContainerTypeId,
                FinalPrice = rate.Price,
                Currency = rate.Currency,
                CreatedAt = now,
                CarrierId = rate.CarrierId,
                RateId = rate.Id,

                RequestedGrossWeightKg = dto.RequestedGrossWeightKg,
                RequestedNetWeightKg = dto.RequestedNetWeightKg,
                RequestedVolumeCbm = dto.RequestedVolumeCbm,
                IsHazardous = dto.IsHazardous,
                RequiredTemperatureCelsius = dto.RequiredTemperatureCelsius,
                RequestedChargeableWeightKg = ShipmentWeightCalculator
                 .CalculateItemChargeableWeight(dto.RequestedGrossWeightKg, dto.RequestedVolumeCbm),

                Status = QuoteStatus.Pending
            };

            await _unitOfWork.Quotes.AddAsync(quote);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Quotes.GetWithItemsAsync(quote.Id);
            return _mapper.Map<QuoteResponse>(created);
        }

        public async Task<QuoteResponse> AcceptFromUserAsync(Guid id, string userId)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (user.CustomerProfile == null)
                throw new BusinessRuleException("User does not have a customer profile.");

            var quote = await _unitOfWork.Quotes
                .GetByIdAndCustomerIdAsync(id, user.CustomerProfile.Id);

            if (quote == null)
                throw new KeyNotFoundException("Quote was not found or does not belong to this customer.");

            if (quote.Status != QuoteStatus.Pending)
                throw new BusinessRuleException($"Only quotes pending customer approval can be accepted. Current status: {quote.Status}.");

            quote.Status = QuoteStatus.Accepted;
            quote.UpdatedAt = DateTimeOffset.UtcNow;

            var shipment = new Shipment
            {
                QuoteId = quote.Id,
                Quote = quote,
                CustomerId = quote.CustomerId,
                RouteId = quote.RouteId,
                ContainerTypeId = quote.ContainerTypeId,
                CarrierId = quote.CarrierId,
                AgreedPrice = quote.FinalPrice,
                Currency = quote.Currency,
                Status = ShipmentStatus.Created,
                CreatedAt = DateTimeOffset.UtcNow,
                AllowedGrossWeightKg = quote.RequestedGrossWeightKg,
                AllowedNetWeightKg = quote.RequestedNetWeightKg,
                AllowedVolumeCbm = quote.RequestedVolumeCbm,
                IsHazardousAllowed = quote.IsHazardous,
                AllowedChargeableWeightKg = quote.RequestedChargeableWeightKg
            };

            await _unitOfWork.Shipments.AddAsync(shipment);

            var invoice = new Invoice
            {
                ShipmentId = shipment.Id,
                Shipment = shipment,
                InvoiceNumber = InvoiceHelper.GenerateInvoiceNumber(shipment.Customer.NationalId!),
                Currency = InvoiceHelper.NormalizeAndValidateCurrency(shipment.Currency),
                NetShipmentPrice = shipment.AgreedPrice,
                SubTotal = shipment.AgreedPrice,
                TaxAmount = 0.14m * shipment.AgreedPrice, // Assuming 14% tax
                TotalAmount = shipment.AgreedPrice + (0.14m * shipment.AgreedPrice),
                PaymentStatus = PaymentStatus.Pending,
                IssuedAt = DateTimeOffset.UtcNow,
                DueDate = DateTimeOffset.UtcNow.AddDays(7),
                CreatedAt = DateTimeOffset.UtcNow,
                PayerType = PayerType.Shipper,
            };

            var charge = new ShipmentCharge
            {
                Shipment = shipment,
                InvoiceId = invoice.Id,
                Invoice = invoice,
                ChargeType = ChargeType.OceanFreight,
                PayerType = invoice.PayerType,
                Description = "Ocean freight charge based on approved quote Acceptance",
                Amount = quote.FinalPrice,
                TaxAmount = invoice.TaxAmount,
                Currency = quote.Currency,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            invoice.Charges.Add(charge);

            await _unitOfWork.ShipmentCharges.AddAsync(charge);

            await _unitOfWork.Invoices.AddAsync(invoice);

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<QuoteResponse>(quote);
        }

        public async Task<QuoteResponse> RejectFromUserAsync(Guid id, string userId, string reason)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
                throw new UnauthorizedAccessException("User is not authenticated.");

            if (user.CustomerProfile == null)
                throw new BusinessRuleException("User does not have a customer profile.");

            var quote = await _unitOfWork.Quotes
                .GetByIdAndCustomerIdAsync(id, user.CustomerProfile.Id);

            if (quote == null)
                throw new KeyNotFoundException("Quote was not found or does not belong to this customer.");

            if (quote.Status != QuoteStatus.Pending)
                throw new BusinessRuleException($"Only quotes pending customer approval can be accepted. Current status: {quote.Status}.");

            quote.Status = QuoteStatus.Rejected;
            quote.UpdatedAt = DateTimeOffset.UtcNow;
            quote.Reason = reason;

            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<QuoteResponse>(quote);
        }

        public async Task DeleteAsync(Guid id, bool isAdmin)
        {
            if(!isAdmin)
                throw new BusinessRuleException("Only staff or administrators can review quote requests.");
            var quote = await _unitOfWork.Quotes.GetWithItemsAsync(id);
            if (quote == null || quote.IsDeleted)
                throw new KeyNotFoundException("Quote not found.");

            quote.IsDeleted = true;
            quote.DeletedAt = DateTimeOffset.UtcNow;
            quote.UpdatedAt = DateTimeOffset.UtcNow;

            await _unitOfWork.SaveChangesAsync();
        }

        private static void ValidateCargoAgainstRate(CreateQuoteRequest dto, Rate rate)
        {
            if (dto.RequestedGrossWeightKg <= 0)
                throw new BusinessRuleException("Gross weight must be greater than zero.");

            if (dto.RequestedNetWeightKg <= 0)
                throw new BusinessRuleException("Net weight must be greater than zero.");

            if (dto.RequestedVolumeCbm <= 0)
                throw new BusinessRuleException("Volume CBM must be greater than zero.");

            if (dto.RequestedNetWeightKg > dto.RequestedGrossWeightKg)
                throw new BusinessRuleException("Net weight cannot be greater than gross weight.");

            if (rate.MaxGrossWeightKg.HasValue &&
                dto.RequestedGrossWeightKg > rate.MaxGrossWeightKg.Value)
                throw new BusinessRuleException($"Gross weight exceeds the allowed limit for this rate. Max allowed: {rate.MaxGrossWeightKg.Value} kg.");

            if (rate.MaxNetWeightKg.HasValue &&
                dto.RequestedNetWeightKg > rate.MaxNetWeightKg.Value)
                throw new BusinessRuleException($"Net weight exceeds the allowed limit for this rate. Max allowed: {rate.MaxNetWeightKg.Value} kg.");

            if (rate.MaxVolumeCbm.HasValue &&
                dto.RequestedVolumeCbm > rate.MaxVolumeCbm.Value)
                throw new BusinessRuleException($"Volume exceeds the allowed limit for this rate. Max allowed: {rate.MaxVolumeCbm.Value} CBM.");

            if (dto.IsHazardous && !rate.AllowsHazardous)
                throw new BusinessRuleException("Hazardous cargo is not allowed for this rate.");

            if (dto.RequiredTemperatureCelsius.HasValue)
            {
                if (!rate.MinTemperatureCelsius.HasValue || !rate.MaxTemperatureCelsius.HasValue)
                    throw new BusinessRuleException("Temperature-controlled cargo is not supported for this rate.");

                if (dto.RequiredTemperatureCelsius.Value < rate.MinTemperatureCelsius.Value ||
                    dto.RequiredTemperatureCelsius.Value > rate.MaxTemperatureCelsius.Value)
                    throw new BusinessRuleException(
                        $"Required temperature is outside the allowed range for this rate. Allowed range: {rate.MinTemperatureCelsius.Value} to {rate.MaxTemperatureCelsius.Value} °C.");
            }
        }
    }
}
