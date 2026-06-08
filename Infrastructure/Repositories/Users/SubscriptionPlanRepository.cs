using Application.Interfaces.Repositories.Users;
using Domain.Entities.Users;
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

        public async Task<IReadOnlyCollection<SubscriptionPlan?>> GetAllAsync()
        {
            var query = _context.SubscriptionPlans.Select(x => x)
                .Where(x => x.IsActive).AsNoTracking();
            return await query.ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetByIdAsync(Guid subscriptionPlanId)
        {
            return await _context.SubscriptionPlans.FirstOrDefaultAsync(x => x.Id == subscriptionPlanId);
        }
    }
}
