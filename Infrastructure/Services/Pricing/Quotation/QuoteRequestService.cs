using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using AutoMapper;
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
    public class QuoteRequestService : IQuoteRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public QuoteRequestService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<QuoteRequestResponse> ApproveAsync(Guid requestId, string userId)
        {
            var user = await ValidateReviewerAsync(userId);

            var request = await _unitOfWork.QuoteRequest.GetById(requestId);
            if (request == null)
                throw new BusinessRuleException("Quote request not found.");

            if (request.Status != QuoteRequestStatus.PendingReview)
                throw new BusinessRuleException("Only pending quote requests can be approved.");

            var quote = new Quote
            {
                CustomerId = request.CustomerId,
                RateId = request.RateId,
                RouteId = request.Rate.RouteId,
                CarrierId = request.Rate.CarrierId,
                ContainerTypeId = request.Rate.ContainerTypeId,
                FinalPrice = request.RequestedRatePrice,
                Currency = request.Currency,
                CreatedAt = DateTimeOffset.UtcNow,
                Status = QuoteStatus.Accepted,
                RequestedGrossWeightKg = request.RequestedGrossWeightKg,
                RequestedNetWeightKg = request.RequestedNetWeightKg,
                RequestedVolumeCbm = request.RequestedVolumeCbm,
                RequiredTemperatureCelsius = request.RequiredTemperatureCelsius,
                RequestedChargeableWeightKg = ShipmentWeightCalculator
                .CalculateItemChargeableWeight(request.RequestedGrossWeightKg, request.RequestedVolumeCbm),
                IsHazardous = request.IsHazardous,
            };

            await _unitOfWork.Quotes.AddAsync(quote);

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
                AllowedGrossWeightKg = request.RequestedGrossWeightKg,
                AllowedNetWeightKg = request.RequestedNetWeightKg,
                AllowedVolumeCbm = request.RequestedVolumeCbm,
                IsHazardousAllowed = request.IsHazardous,
                AllowedChargeableWeightKg = ShipmentWeightCalculator
                .CalculateItemChargeableWeight(request.RequestedGrossWeightKg, request.RequestedVolumeCbm),
            };

            await _unitOfWork.Shipments.AddAsync(shipment);

            return await UpdateRequestStatusAsync(request, user, QuoteRequestStatus.Approved);
        }

        public async Task<QuoteRequestResponse> CancelByUserAsync(Guid requestId, string userId)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new BusinessRuleException("User not found.");

            var request = await _unitOfWork.QuoteRequest.GetMyRequestById(user.CustomerProfile.Id, requestId);

            if (request == null)
                throw new BusinessRuleException("Quote request not found.");

            return await UpdateRequestStatusAsync(request, user, QuoteRequestStatus.Cancelled);
        }

        public async Task<QuoteRequestResponse> RejectAsync(Guid requestId, string userId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new BusinessRuleException("Rejection reason is required.");

            var user = await ValidateReviewerAsync(userId);

            var request = await _unitOfWork.QuoteRequest.GetById(requestId);

            if (request == null)
                throw new BusinessRuleException("Quote request not found.");

            return await UpdateRequestStatusAsync(request, user, QuoteRequestStatus.Rejected, reason);
        }

        public async Task<QuoteRequestResponse> CreateFromRateAsync(CreateQuoteRequestFromRate request, string userId)
        {
            var user = await _userManager.Users
                .Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new BusinessRuleException("User not found.");

            var rate = await _unitOfWork.Rates.GetByIdWithDetailsAsync(request.RateId);

            if (rate == null || rate.IsDeleted)
                throw new BusinessRuleException("Rate not found.");

            var requestExsits = await _unitOfWork.QuoteRequest.HasPendingRequestForRateAsync(user.CustomerProfile.Id, rate.Id);
            if (requestExsits)
                throw new BusinessRuleException("You already have a pending quote request for this rate.");

            var now = DateTimeOffset.UtcNow;

            if (!rate.IsActive)
                throw new BusinessRuleException("This rate is no longer active.");

            if (rate.ValidFrom > now || rate.ValidTo < now)
                throw new BusinessRuleException("This rate is not valid at the current time.");

            if (request.RequiredTemperatureCelsius.HasValue && !rate.ContainerType.Name
            .Contains("Reefer", StringComparison.OrdinalIgnoreCase))
            {
                throw new BusinessRuleException("Temperature-controlled cargo requires a reefer container.");
            }

            if (request.RequestedGrossWeightKg > rate.MaxGrossWeightKg)
                throw new BusinessRuleException(
                    "Requested gross weight exceeds the maximum allowed gross weight for this rate.");

            if (request.RequestedNetWeightKg > rate.MaxNetWeightKg)
                throw new BusinessRuleException(
                    "Requested net weight exceeds the maximum allowed net weight for this rate.");

            if (request.RequestedVolumeCbm > rate.MaxVolumeCbm)
                throw new BusinessRuleException(
                    "Requested volume exceeds the maximum allowed volume for this rate.");

            var quoteRequest = _mapper.Map<QuoteRequest>(request);

            quoteRequest.CustomerId = user.CustomerProfile.Id;
            quoteRequest.RateId = rate.Id;

            quoteRequest.RequestedRatePrice = rate.Price;
            quoteRequest.Currency = rate.Currency.Trim().ToUpper();

            quoteRequest.Status = QuoteRequestStatus.PendingReview;
            quoteRequest.CreatedAt = now;

            await _unitOfWork.QuoteRequest.AddAsync(quoteRequest);
            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<QuoteRequestResponse>(quoteRequest);

            return dto;
        }

        public async Task<IEnumerable<QuoteRequestResponse>> GetAllAsync(string userId, QueryParameters query)
        {
            var user = await ValidateReviewerAsync(userId);

            var quotes = await _unitOfWork.QuoteRequest.GetAllAsync(query);
            if(!quotes.Any())
                return new List<QuoteRequestResponse>();

            return _mapper.Map<IEnumerable<QuoteRequestResponse>>(quotes);
        }

        public async Task<QuoteRequestResponse> GetByIdAsync(Guid id)
        {
            var request = await _unitOfWork.QuoteRequest.GetById(id);

            if (request == null)
                throw new BusinessRuleException("Quote request not found.");

            return _mapper.Map<QuoteRequestResponse>(request);
        }

        public async Task<IEnumerable<QuoteRequestResponse>> GetMyRequestsAsync(string userId, QueryParameters query)
        {
            var user = await _userManager.Users
               .Include(x => x.CustomerProfile)
               .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new BusinessRuleException("User not found.");

            var quotes = await _unitOfWork.QuoteRequest.GetMyRequests(user.CustomerProfile.Id, query);
            if (!quotes.Any())
                return new List<QuoteRequestResponse>();

            return _mapper.Map<IEnumerable<QuoteRequestResponse>>(quotes);
        }

        private async Task<QuoteRequestResponse> UpdateRequestStatusAsync(QuoteRequest request, ApplicationUser reviewer,
        QuoteRequestStatus newStatus, string? rejectionReason = null)
        {
            switch (request.Status)
            {
                case QuoteRequestStatus.Approved:
                    throw new BusinessRuleException("This quote request has already been approved.");

                case QuoteRequestStatus.Rejected:
                    throw new BusinessRuleException("This quote request has already been rejected.");

                case QuoteRequestStatus.Cancelled:
                    throw new BusinessRuleException("This quote request has been cancelled.");
            }

            request.Status = newStatus;
            request.ReviewedAt = DateTimeOffset.UtcNow;
            request.ReviewedByUserId = reviewer.Id;

            if (newStatus == QuoteRequestStatus.Rejected)
            {
                request.RejectionReason = rejectionReason?.Trim();
            }

            await _unitOfWork.SaveChangesAsync();

            var dto = _mapper.Map<QuoteRequestResponse>(request);

            dto.ReviewedByUserName = $"{reviewer.FirstName} {reviewer.LastName}";

            return dto;
        }

        private async Task<ApplicationUser> ValidateReviewerAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new BusinessRuleException("User not found.");

            var isAdminOrStaff =
                await _userManager.IsInRoleAsync(user, "Admin") ||
                await _userManager.IsInRoleAsync(user, "Staff");

            if (!isAdminOrStaff)
                throw new BusinessRuleException("Only admins or staff members can review quote requests.");

            return user;
        }
    }
}
