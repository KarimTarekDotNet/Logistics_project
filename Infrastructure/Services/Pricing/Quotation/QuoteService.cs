using Application.ApplicationRules;
using Application.DTOs.Pricing.Quotation;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Models;
using AutoMapper;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Users;
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
            var user = await _userManager.FindByIdAsync(userId);
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
            var route = await _unitOfWork.Routes.GetByIdAsync(dto.RouteId);
            if (route == null || route.IsDeleted)
                throw new KeyNotFoundException("Route not found.");

            var containerType = await _unitOfWork.ContainerTypes.GetByIdAsync(dto.ContainerTypeId);
            if (containerType == null || containerType.IsDeleted)
                throw new KeyNotFoundException("Container type not found.");

            if (!QuoteRules.IsFinalPriceConsistent(dto.FinalPrice, dto.Items.Select(i => i.Amount)))
                throw new ArgumentException("Final price must be greater than or equal to the sum of all item amounts.");

            var quote = _mapper.Map<Quote>(dto);
            quote.CreatedAt = DateTimeOffset.UtcNow;

            if (quote.Items != null && quote.Items.Any())
            {
                foreach (var item in quote.Items)
                {
                    item.CreatedAt = DateTimeOffset.UtcNow;
                }
            }

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