using Domain.Entities.Users;

namespace Application.Interfaces.Repositories.Users
{
    public interface ISubscriptionPlanRepository
    {
        Task<SubscriptionPlan?> GetByIdAsync(Guid subscriptionPlanId);
        Task<IReadOnlyCollection<SubscriptionPlan?>> GetAllAsync();

        Task AddAsync(SubscriptionPlan subscriptionPlan);
        void Delete(SubscriptionPlan subscriptionPlan);
    }
    public interface IUserSubscriptionRepository
    {
        Task<IReadOnlyCollection<UserSubscription?>> GetByUserIdAsync(string userId);
        Task<UserSubscription?> GetCurrentSubscriptionByUserIdAsync(string userId);
        Task AddAsync(UserSubscription userSubscription);
        void Delete(UserSubscription userSubscription);
        Task<bool> ExistsAsyncForUser(string userId, Guid subId);
    }
}