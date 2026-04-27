using Domain.Entities.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Data
{
    public static class SeedRoles
    {
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            List<string> roles = new List<string>();

            foreach (var role in Enum.GetValues(typeof(Domain.Enums.Role)))
            {
                roles.Add(role.ToString()!);
            }

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            // Admin
            var admin = new ApplicationUser
            {
                UserName = "admin@system.com",
                Email = "admin@system.com",
                FirstName = "System",
                LastName = "Admin",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(admin.Email) == null)
            {
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Staff
            var staff = new ApplicationUser
            {
                UserName = "staff@system.com",
                Email = "staff@system.com",
                FirstName = "System",
                LastName = "Staff",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(staff.Email) == null)
            {
                await userManager.CreateAsync(staff, "Staff@123");
                await userManager.AddToRoleAsync(staff, "Staff");
            }

            // Customer
            var customer = new ApplicationUser
            {
                UserName = "customer@system.com",
                Email = "customer@system.com",
                FirstName = "System",
                LastName = "Customer",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(customer.Email) == null)
            {
                await userManager.CreateAsync(customer, "Customer@123");
                await userManager.AddToRoleAsync(customer, "Customer");
            }

            // Integration (اختياري)
            var integration = new ApplicationUser
            {
                UserName = "integration@system.com",
                Email = "integration@system.com",
                FirstName = "System",
                LastName = "Integration",
                EmailConfirmed = true
            };

            if (await userManager.FindByEmailAsync(integration.Email) == null)
            {
                await userManager.CreateAsync(integration, "Integration@123");
                await userManager.AddToRoleAsync(integration, "Integration");
            }
        }
    }
}
