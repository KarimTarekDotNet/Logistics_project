using Application.Interfaces.Repositories.Users;
using Domain.Entities.Users;
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
                .Where(us => us.UserId == userId && !us.IsDeleted)
                .ToListAsync();
        }

        public async Task<UserSubscription?> GetCurrentSubscriptionByUserIdAsync(string userId)
        {
            return await _context.UserSubscriptions.AsNoTracking()
                .Include(x => x.SubscriptionPlan)
                .Where(us => us.UserId == userId && !us.IsDeleted && us.EndDate > DateTimeOffset.UtcNow)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> ExistsAsyncForUser(string userId, Guid subId)
        {
            return await _context.UserSubscriptions.Include(x => x.SubscriptionPlan).AnyAsync(us => us.UserId == userId
            && !us.IsDeleted && us.EndDate > DateTimeOffset.UtcNow && us.SubscriptionPlanId == subId);
        }
    }
}