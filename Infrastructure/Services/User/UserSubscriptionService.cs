using Application.DTOs.User;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Users;
using AutoMapper;
using Domain.Entities.Users;
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

        public Task<UserSubscriptionResponse> SubscribeUserToPlanAsync(string userId, Guid supId)
        {
            throw new NotImplementedException();
        }
    }
}
