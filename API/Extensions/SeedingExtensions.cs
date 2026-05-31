using Domain.Entities.Users;
using Infrastructure.Data.Configuration.Seeding;
using Infrastructure.Data.Database;
using Microsoft.AspNetCore.Identity;

namespace API.Extensions
{
    public static class SeedingExtensions
    {
        public static async Task<WebApplication> SeedDatabaseAsync(this WebApplication app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = services.GetRequiredService<ApplicationDbContext>();

                await AppSeeder.SeedAsync(roleManager, userManager, dbContext);
            }
            return app;
        }
    }
}
