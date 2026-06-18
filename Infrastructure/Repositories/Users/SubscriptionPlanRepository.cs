using Application.Interfaces.Repositories.Users;
using Domain.Entities.Users.Subscriptions;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Users
{
    public class SubscriptionPlanRepository : ISubscriptionPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SubscriptionPlan subscriptionPlan)
        {
            await _context.SubscriptionPlans.AddAsync(subscriptionPlan);
        }

        public void Delete(SubscriptionPlan subscriptionPlan)
        {
            subscriptionPlan.DeletedAt = DateTimeOffset.UtcNow;
            subscriptionPlan.IsDeleted = true;
        }

        public Task<IReadOnlyCollection<SubscriptionPlan?>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionFeature?> GetByCodeAsync(string code)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlan?> GetByIdAsync(Guid subscriptionPlanId)
        {
            throw new NotImplementedException();
        }

        public Task<SubscriptionPlanLimit?> GetSubscriptionPlanLimitByIdAsync(Guid subscriptionPlanId)
        {
            throw new NotImplementedException();
        }
    }
}
