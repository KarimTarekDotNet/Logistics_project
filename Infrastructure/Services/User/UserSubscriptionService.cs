using Application.DTOs.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Users;
using AutoMapper;
using Domain.Entities.Users;
using Domain.Entities.Users.Subscriptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.User
{
    public class UserSubscriptionService : IUserSubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserSubscriptionService(IUnitOfWork unitOfWork, IMapper mapper, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<UserSubscriptionResponse?> GetCurrentUserSubscriptionsAsync(string userId)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
            .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new UnauthorizedAccessException("");

            var currentPlanForUser = await _unitOfWork.UserSubscriptions.GetCurrentSubscriptionByUserIdAsync(userId);

            if(currentPlanForUser == null)
                return null;

            var dto = _mapper.Map<UserSubscriptionResponse>(currentPlanForUser);

            dto.Username = user.FirstName + " " + user.LastName;

            dto.SubscriptionPlanTitle = currentPlanForUser.SubscriptionPlan.Title;

            return dto;
        }

        public async Task<IReadOnlyCollection<UserSubscriptionResponse>> GetUserSubscriptionsAsync(string userId)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new UnauthorizedAccessException("");

            var userSubscriptions = await _unitOfWork.UserSubscriptions.GetByUserIdAsync(userId);

            if (!userSubscriptions.Any())
                return new List<UserSubscriptionResponse>();

            var subscriptions = _mapper.Map<List<UserSubscriptionResponse>>(userSubscriptions);

            foreach (var subscription in subscriptions)
            {
                var source = userSubscriptions.First(x => x!.Id == subscription.Id)!;
                subscription.Username = user.FirstName + " " + user.LastName;
                subscription.SubscriptionPlanTitle = source.SubscriptionPlan.Title;
            }

            return subscriptions;
        }

        public async Task<UserSubscriptionResponse> SubscribeUserToPlanAsync(string userId, Guid supId)
        {
            var user = await _userManager.Users.Include(x => x.CustomerProfile)
                .FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null || user.CustomerProfile == null)
                throw new UnauthorizedAccessException("");

            var plan = await _unitOfWork.SubscriptionPlans.GetByIdAsync(supId);
            if (plan == null || plan.IsDeleted || !plan.IsActive)
                throw new KeyNotFoundException("Subscription plan not found.");

            var alreadySubscribe = await _unitOfWork.UserSubscriptions.ExistsAsyncForUser(userId, supId);
            if (alreadySubscribe)
                throw new BusinessRuleException("");

            var newSub = new UserSubscription
            {
                Id = Guid.NewGuid(),
                StartDate = DateTimeOffset.UtcNow,
                EndDate = DateTimeOffset.UtcNow.AddDays(plan.DurationInDays),
                IsActive = true,
                SubscriptionPlanId = supId,
                CreatedAt = DateTimeOffset.UtcNow,
                UserId = user.Id,
            };

            foreach (var limit in plan.PlanLimits)
            {
                newSub.Usages.Add(new UserSubscriptionUsage
                {
                    Id = Guid.NewGuid(),
                    LimitCode = limit.LimitCodeSubscription,
                    PeriodStart = newSub.StartDate,
                    PeriodEnd = newSub.EndDate,
                    UsedValue = 0
                });
            }

            await _unitOfWork.UserSubscriptions.AddAsync(newSub);

            var dto = _mapper.Map<UserSubscriptionResponse>(newSub);

            dto.Username = user.FirstName + " " + user.LastName;

            dto.SubscriptionPlanTitle = plan.Title;

            dto.Usages = _mapper.Map<ICollection<UserSubscriptionUsageResponse>>(newSub.Usages);

            return dto;
        }
    }
}
