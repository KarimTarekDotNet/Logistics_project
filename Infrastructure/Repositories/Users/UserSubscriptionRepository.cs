using Application.Interfaces.Repositories.Users;
using Domain.Entities.Users.Subscriptions;
using Infrastructure.Data.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Users
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public UserSubscriptionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserSubscription userSubscription)
        {
            await _context.UserSubscriptions.AddAsync(userSubscription);
        }

        public void Delete(UserSubscription userSubscription)
        {
            userSubscription.DeletedAt = DateTimeOffset.UtcNow;
            userSubscription.IsDeleted = true;
        }

        public async Task<IReadOnlyCollection<UserSubscription?>> GetByUserIdAsync(string userId)
        {
            return await _context.UserSubscriptions.AsNoTracking()
                .Include(x => x.SubscriptionPlan)
                .Include(x => x.Usages)
                .Where(us => us.UserId == userId && !us.IsDeleted)
                .OrderByDescending(us => us.CreatedAt)
                .ToListAsync();
        }
        public async Task<UserSubscriptionUsage?> GetUserSubscriptionUsageByUserSubIdAsync(Guid userSubId)
        {
            return await _context.UserSubscriptionUsages.FirstOrDefaultAsync(x => x.UserSubscriptionId == userSubId);
        }

        public async Task<UserSubscription?> GetCurrentSubscriptionByUserIdAsync(string userId)
        {
            return await _context.UserSubscriptions.AsNoTracking()
                .Include(x => x.SubscriptionPlan)
                .ThenInclude(x => x.Features)
                .Include(x => x.Usages)
                .Where(us => us.UserId == userId && !us.IsDeleted && us.IsActive && us.EndDate > DateTimeOffset.UtcNow)
                .OrderByDescending(us => us.CreatedAt)
                .FirstOrDefaultAsync();
        }



        public async Task<bool> ExistsAsyncForUser(string userId, Guid subId)
        {
            return await _context.UserSubscriptions.Include(x => x.SubscriptionPlan).AnyAsync(us => us.UserId == userId
            && !us.IsDeleted && us.EndDate > DateTimeOffset.UtcNow && us.SubscriptionPlanId == subId);
        }
    }
}
