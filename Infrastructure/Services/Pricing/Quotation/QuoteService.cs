using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using AutoMapper;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Users;
using Domain.Exceptions;
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

        public async Task<QuoteResponse> CreateAsync(CreateQuoteRequest dto)
        {
            var customer = await _unitOfWork.Customers.GetDetailsByIdAsync(dto.CustomerId);
            if(customer == null)
                throw new KeyNotFoundException("Customer not found.");

            var rate = await _unitOfWork.Rates.GetByIdAsync(dto.RateId);
            if (rate == null || rate.IsDeleted)
                throw new KeyNotFoundException("Rate not found.");

            var now = DateTimeOffset.UtcNow;

            if (!rate.IsActive)
                throw new BusinessRuleException("Rate is not active.");

            if (rate.ValidFrom > now || rate.ValidTo < now)
                throw new BusinessRuleException("Rate is not valid at the current time.");

            var extraChargesTotal = dto.Items.Sum(x => x.Amount);

            var quote = new Quote
            {
                CustomerId = customer.Id,
                RouteId = rate.RouteId,
                ContainerTypeId = rate.ContainerTypeId,
                FinalPrice = rate.Price + extraChargesTotal,
                Currency = rate.Currency,
                CreatedAt = now,
                CarrierId = rate.CarrierId,
                RateId = rate.Id,
                Items = dto.Items.Select(item => new QuoteItem
                {
                    Description = item.Description,
                    Amount = item.Amount,
                    CreatedAt = now
                }).ToList()
            };

            await _unitOfWork.Quotes.AddAsync(quote);
            await _unitOfWork.SaveChangesAsync();

            var created = await _unitOfWork.Quotes.GetWithItemsAsync(quote.Id);
            return _mapper.Map<QuoteResponse>(created);
        }

        public async Task DeleteAsync(Guid id)
        {
            var quote = await _unitOfWork.Quotes.GetWithItemsAsync(id);
            if (quote == null || quote.IsDeleted)
                throw new KeyNotFoundException("Quote not found.");

            quote.IsDeleted = true;
            quote.DeletedAt = DateTimeOffset.UtcNow;
            quote.UpdatedAt = DateTimeOffset.UtcNow;

            if (quote.Items != null && quote.Items.Any())
            {
                foreach (var item in quote.Items.Where(i => !i.IsDeleted))
                {
                    item.IsDeleted = true;
                    item.DeletedAt = DateTimeOffset.UtcNow;
                    item.UpdatedAt = DateTimeOffset.UtcNow;
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }
    }
}