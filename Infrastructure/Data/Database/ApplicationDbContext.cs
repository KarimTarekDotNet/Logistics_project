using Domain.Entities.Aliases;
using Domain.Entities.Payments;
using Domain.Entities.Pricing.Imports;
using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;
using Domain.Entities.Users.Subscriptions;
using Infrastructure.Data.Configuration.Pricing;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Database
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
        public DbSet<IntegrationMessage> IntegrationMessages { get; set; }

        // Quotation
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteRequest> QuoteRequests { get; set; }

        // shipment
        public DbSet<Shipment> Shipments { get; set; }
        public DbSet<ShipmentItem> ShipmentItems { get; set; }
        public DbSet<ShipmentStatusHistory> ShipmentStatusHistories { get; set; }
        public DbSet<ShipmentCharge> ShipmentCharges { get; set; }
        public DbSet<ShipmentChargeItem> ShipmentChargesItems { get; set; }
        public DbSet<ShipmentChargeRule> ShipmentChargeRules { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<ShipmentDocument> ShipmentDocuments { get; set; }
        public DbSet<Customer> Customers { get; set; }

        // Payments
        public DbSet<InvoicePayment> InvoicePayments { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }

        // Subscriptions
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<SubscriptionFeature> SubscriptionFeatures { get; set; }
        public DbSet<SubscriptionPlanFeature> SubscriptionPlanFeatures { get; set; }
        public DbSet<SubscriptionPlanLimit> SubscriptionPlanLimit { get; set; }
        public DbSet<UserSubscription> UserSubscriptions { get; set; }
        public DbSet<UserSubscriptionUsage> UserSubscriptionUsages { get; set; }

        // Users
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }


        // Aliases
        public DbSet<Alias> Aliases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global query filters for soft delete
            modelBuilder.Entity<Rate>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Quote>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<QuoteRequest>().HasQueryFilter(r => !r.Customer.IsDeleted && !r.Rate.IsDeleted);

            modelBuilder.Entity<Carrier>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<ContainerType>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Port>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Route>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Shipment>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<ShipmentItem>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<ShipmentCharge>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<ShipmentStatusHistory>().HasQueryFilter(r => !r.Shipment.IsDeleted);
            modelBuilder.Entity<ShipmentDocument>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<SubscriptionPlan>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<UserSubscription>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Invoice>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Customer>().HasQueryFilter(r => !r.IsDeleted);

            modelBuilder.Entity<Alias>().HasQueryFilter(r => !r.IsDeleted);


            modelBuilder.Entity<Alias>().Property(r => r.Type).HasConversion<string>();
            modelBuilder.Entity<QuoteRequest>().Property(r => r.Status).HasConversion<string>();
            modelBuilder.Entity<Quote>().Property(r => r.Status).HasConversion<string>();

            // indexes
            modelBuilder.Entity<ApplicationUser>().HasIndex(x => x.PhoneNumber).IsUnique().HasFilter("[PhoneNumber] IS NOT NULL");
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.HashedToken).IsUnique();
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.ApplicationUserId);
            modelBuilder.Entity<IntegrationMessage>().HasIndex(x => new {x.ExternalMessageId, x.Source}).IsUnique();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RateConfiguration).Assembly);

        }
    }
}
