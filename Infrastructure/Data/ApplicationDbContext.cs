using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;
using Infrastructure.Data.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        // Shipping Core
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<Port> Ports { get; set; }
        public DbSet<Route> Routes { get; set; }
        public DbSet<ContainerType> ContainerTypes { get; set; }

        // Pricing Engine
        public DbSet<Rate> Rates { get; set; }

        // Quotation
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }

        // Users
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filters for soft delete
            modelBuilder.Entity<Rate>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Quote>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<QuoteItem>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Carrier>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<ContainerType>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Port>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Route>().HasQueryFilter(r => !r.IsDeleted);

            // indexes
            modelBuilder.Entity<ApplicationUser>().HasIndex(x => x.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.HashedToken).IsUnique();
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.ApplicationUserId);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RateConfiguration).Assembly);

        }
    }
}