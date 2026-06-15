using Application.DTOs.User;

namespace Application.Interfaces.Repositories.Users
{
    public interface ISubscriptionPlanService
    {
        Task<SubscriptionPlanResponse> AddFromEmployeesAsync(CreateSubscriptionPlanRequest request, bool isEmployee);
        Task<SubscriptionPlanResponse> UpdateFromEmployeesAsync(Guid subscriptionPlanId, bool isEmployee, CreateSubscriptionPlanRequest? request = null);
        Task DeleteFromEmployeesAsync(Guid subscriptionPlanId, bool isEmployee);
        Task<IReadOnlyCollection<SubscriptionPlanResponse>> GetAllAsync();
        Task<SubscriptionPlanResponse> GetByIdAsync(Guid subscriptionPlanId);
    }

    public interface IUserSubscriptionService
    {
        Task<UserSubscriptionResponse> SubscribeUserToPlanAsync(string userId, Guid supId);
        Task<IReadOnlyCollection<UserSubscriptionResponse>> GetUserSubscriptionsAsync(string userId);
        Task<UserSubscriptionResponse?> GetCurrentUserSubscriptionsAsync(string userId);
    }
}
