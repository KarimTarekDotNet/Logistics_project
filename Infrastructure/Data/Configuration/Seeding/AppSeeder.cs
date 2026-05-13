using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.Shipments;
using Domain.Entities.ShippingCore;
using Domain.Entities.Users;
using Domain.Enums;
using Infrastructure.Data.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Configuration.Seeding
{
    public static class AppSeeder
    {
        public static async Task SeedAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            // =========================
            // 1. Roles
            // =========================
            foreach (var role in Enum.GetValues(typeof(Role)))
            {
                var roleName = role.ToString()!;
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // =========================
            // 2. Users
            // =========================
            async Task<ApplicationUser> CreateUserIfNotExists(string email, string username , string firstName, string lastName, string role, string? fixedId = null)
            {
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser
                    {
                        Id = fixedId ?? Guid.NewGuid().ToString(),
                        UserName = username,
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        EmailConfirmed = true
                    };
                    await userManager.CreateAsync(user, $"{role}@123");
                    await userManager.AddToRoleAsync(user, role);
                }
                return user;
            }

            await CreateUserIfNotExists("admin@system.com", "admin@system", "System", "Admin", "Admin");
            await CreateUserIfNotExists("staff@system.com", "staff@ystem", "Staff", "Staff", "Staff");

            var customerUser = await CreateUserIfNotExists("user@system.com", "user@system", "System", "User", "User");

            await CreateUserIfNotExists("integration@system.com", "System", "integration@system",  "Integration", "Integration");

            // =========================
            // 3. Ports
            // =========================
            var seedDate = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

            var portShanghaiId = new Guid("00000000-0000-0000-0000-000000000001");
            var portRotterdamId = new Guid("00000000-0000-0000-0000-000000000002");
            var portDubaiId = new Guid("00000000-0000-0000-0000-000000000003");

            if (!await db.Ports.AnyAsync(p => p.Id == portShanghaiId))
                db.Ports.Add(new Port { Id = portShanghaiId, Name = "Shanghai", Code = "CNSHA", Country = "China", IsDeleted = false, CreatedAt = seedDate });

            if (!await db.Ports.AnyAsync(p => p.Id == portRotterdamId))
                db.Ports.Add(new Port { Id = portRotterdamId, Name = "Rotterdam", Code = "NLRTM", Country = "Netherlands", IsDeleted = false, CreatedAt = seedDate });

            if (!await db.Ports.AnyAsync(p => p.Id == portDubaiId))
                db.Ports.Add(new Port { Id = portDubaiId, Name = "Dubai", Code = "AEJEA", Country = "UAE", IsDeleted = false, CreatedAt = seedDate });

            await db.SaveChangesAsync();

            // =========================
            // 4. Carriers
            // =========================
            var carrierMaerskId = new Guid("00000000-0000-0000-0000-000000000010");
            var carrierMscId = new Guid("00000000-0000-0000-0000-000000000011");

            if (!await db.Carriers.AnyAsync(c => c.Id == carrierMaerskId))
                db.Carriers.Add(new Carrier { Id = carrierMaerskId, Name = "Maersk Line", Code = "MAEU", IsDeleted = false, CreatedAt = seedDate });

            if (!await db.Carriers.AnyAsync(c => c.Id == carrierMscId))
                db.Carriers.Add(new Carrier { Id = carrierMscId, Name = "Mediterranean Shipping Company", Code = "MSCU", IsDeleted = false, CreatedAt = seedDate });

            await db.SaveChangesAsync();

            // =========================
            // 5. Container Types
            // =========================
            var container20FtId = new Guid("00000000-0000-0000-0000-000000000020");
            var container40FtId = new Guid("00000000-0000-0000-0000-000000000021");
            var container40HqId = new Guid("00000000-0000-0000-0000-000000000022");

            if (!await db.ContainerTypes.AnyAsync(c => c.Id == container20FtId))
                db.ContainerTypes.Add(new ContainerType { Id = container20FtId, Name = "20ft Standard", IsDeleted = false, CreatedAt = seedDate });

            if (!await db.ContainerTypes.AnyAsync(c => c.Id == container40FtId))
                db.ContainerTypes.Add(new ContainerType { Id = container40FtId, Name = "40ft Standard", IsDeleted = false, CreatedAt = seedDate });

            if (!await db.ContainerTypes.AnyAsync(c => c.Id == container40HqId))
                db.ContainerTypes.Add(new ContainerType { Id = container40HqId, Name = "40ft High Cube", IsDeleted = false, CreatedAt = seedDate });

            await db.SaveChangesAsync();

            // =========================
            // 6. Routes
            // =========================
            var routeShanghaiRotterdamId = new Guid("00000000-0000-0000-0000-000000000030");
            var routeShanghaiDubaiId = new Guid("00000000-0000-0000-0000-000000000031");

            if (!await db.Routes.AnyAsync(r => r.Id == routeShanghaiRotterdamId))
                db.Routes.Add(new Route { Id = routeShanghaiRotterdamId, FromPortId = portShanghaiId, ToPortId = portRotterdamId, IsDeleted = false, CreatedAt = seedDate });

            if (!await db.Routes.AnyAsync(r => r.Id == routeShanghaiDubaiId))
                db.Routes.Add(new Route { Id = routeShanghaiDubaiId, FromPortId = portShanghaiId, ToPortId = portDubaiId, IsDeleted = false, CreatedAt = seedDate });

            await db.SaveChangesAsync();

            // =========================
            // 7. Rates
            // =========================
            var rateUpdated = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero);
            var validFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

            var rate1Id = new Guid("00000000-0000-0000-0000-000000000040");
            var rate2Id = new Guid("00000000-0000-0000-0000-000000000041");
            var rate3Id = new Guid("00000000-0000-0000-0000-000000000042");

            if (!await db.Rates.AnyAsync(r => r.Id == rate1Id))
                db.Rates.Add(new Rate { Id = rate1Id, CarrierId = carrierMaerskId, RouteId = routeShanghaiRotterdamId, ContainerTypeId = container20FtId, Price = 1500.00m, Currency = "USD", ValidFrom = validFrom, ValidTo = validTo, IsActive = true, IsDeleted = false, CreatedAt = seedDate, UpdatedAt = rateUpdated });

            if (!await db.Rates.AnyAsync(r => r.Id == rate2Id))
                db.Rates.Add(new Rate { Id = rate2Id, CarrierId = carrierMaerskId, RouteId = routeShanghaiRotterdamId, ContainerTypeId = container40FtId, Price = 2800.00m, Currency = "USD", ValidFrom = validFrom, ValidTo = validTo, IsActive = false, IsDeleted = false, CreatedAt = seedDate, UpdatedAt = rateUpdated });

            if (!await db.Rates.AnyAsync(r => r.Id == rate3Id))
                db.Rates.Add(new Rate { Id = rate3Id, CarrierId = carrierMscId, RouteId = routeShanghaiDubaiId, ContainerTypeId = container20FtId, Price = 900.00m, Currency = "USD", ValidFrom = validFrom, ValidTo = validTo, IsActive = true, IsDeleted = false, CreatedAt = seedDate });

            await db.SaveChangesAsync();

            // =========================
            // 8. Customer
            // =========================
            var customer = await db.Customers.FirstOrDefaultAsync(x => x.ApplicationUserId == customerUser.Id);
            if (customer == null)
            {
                customer = new Customer
                {
                    Id = new Guid("00000000-0000-0000-0000-0000000000C1"),
                    NationalId = "29801011234567",
                    ApplicationUserId = customerUser.Id,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false,
                    CompanyName = "Acme Corporation",
                    DateOfBirth = new DateOnly(1990, 1, 1),
                    TaxNumber = "TAX123456789"
                };
                db.Customers.Add(customer);
                await db.SaveChangesAsync();
            }

            // =========================
            // 9. Quotes + Quote Items
            // =========================
            var quoteAlphaId = new Guid("00000000-0000-0000-0000-000000000060");
            var quoteBetaId = new Guid("00000000-0000-0000-0000-000000000061");

            if (!await db.Quotes.AnyAsync(q => q.Id == quoteAlphaId))
            {
                db.Quotes.Add(new Quote
                {
                    Id = quoteAlphaId,
                    CustomerId = customer.Id,

                    CarrierId = carrierMaerskId,
                    RateId = rate1Id,

                    RouteId = routeShanghaiRotterdamId,
                    ContainerTypeId = container20FtId,
                    FinalPrice = 1650.00m,
                    Currency = "USD",
                    IsDeleted = false,
                    CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
                });

                db.QuoteItems.AddRange(
                    new QuoteItem { Id = new Guid("00000000-0000-0000-0000-000000000070"), QuoteId = quoteAlphaId, Description = "Ocean Freight", Amount = 1500.00m, IsDeleted = false, CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero) },
                    new QuoteItem { Id = new Guid("00000000-0000-0000-0000-000000000071"), QuoteId = quoteAlphaId, Description = "Bunker Adjustment Factor", Amount = 150.00m, IsDeleted = false, CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero) }
                );
            }

            if (!await db.Quotes.AnyAsync(q => q.Id == quoteBetaId))
            {
                db.Quotes.Add(new Quote
                {
                    Id = quoteBetaId,
                    CustomerId = customer.Id,

                    CarrierId = carrierMaerskId,
                    RateId = rate2Id,

                    RouteId = routeShanghaiRotterdamId,
                    ContainerTypeId = container40FtId,
                    FinalPrice = 3100.00m,
                    Currency = "USD",
                    IsDeleted = false,
                    CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
                });

                db.QuoteItems.AddRange(
                    new QuoteItem { Id = new Guid("00000000-0000-0000-0000-000000000072"), QuoteId = quoteBetaId, Description = "Ocean Freight", Amount = 2800.00m, IsDeleted = false, CreatedAt = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero) },
                    new QuoteItem { Id = new Guid("00000000-0000-0000-0000-000000000073"), QuoteId = quoteBetaId, Description = "Port Handling Fee", Amount = 300.00m, IsDeleted = false, CreatedAt = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero) }
                );
            }

            await db.SaveChangesAsync();

            // =========================
            // 10. Shipment (Aggregate Root)
            // =========================
            var shipmentId = new Guid("00000000-0000-0000-0000-000000000080");

            var shipment = await db.Shipments
                .Include(x => x.Items)
                .Include(x => x.Charges)
                .Include(x => x.StatusHistory)
                .FirstOrDefaultAsync(x => x.Id == shipmentId);

            if (shipment == null)
            {
                shipment = new Shipment
                {
                    Id = shipmentId,
                    QuoteId = quoteAlphaId,
                    CustomerId = customer.Id,
                    RouteId = routeShanghaiRotterdamId,
                    ContainerTypeId = container20FtId,
                    CarrierId = carrierMaerskId,
                    AgreedPrice = 1650.00m,
                    Currency = "USD",
                    Status = ShipmentStatus.ClientConfirmed,
                    CreatedAt = DateTimeOffset.UtcNow,
                    ClientConfirmedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };

                shipment.Items.Add(new ShipmentItem
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000083"),
                    Description = "Textile Goods",
                    Quantity = 20,
                    ChargeableWeight = 500,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                });

                shipment.Charges.Add(new ShipmentCharge
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000081"),
                    Description = "Ocean Freight",
                    Amount = 1500,
                    TaxAmount = 0,
                    Currency = "USD",
                    ChargeType = ChargeType.OceanFreight,
                    PayerType = PayerType.Shipper,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                });

                shipment.Charges.Add(new ShipmentCharge
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000082"),
                    Description = "Bunker Adjustment Factor",
                    Amount = 150,
                    TaxAmount = 0,
                    Currency = "USD",
                    ChargeType = ChargeType.Other,
                    PayerType = PayerType.Shipper,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                });

                shipment.StatusHistory.Add(new ShipmentStatusHistory
                {
                    Id = new Guid("00000000-0000-0000-0000-000000000084"),
                    FromStatus = ShipmentStatus.Created,
                    ToStatus = ShipmentStatus.ClientConfirmed,
                    ChangedAt = DateTimeOffset.UtcNow,
                    ChangedBy = "System",
                    Reason = "Client confirmed quote"
                });

                db.Shipments.Add(shipment);
                await db.SaveChangesAsync();
            }

            // =========================
            // 11. Invoice
            // =========================

            var invoiceId = new Guid("00000000-0000-0000-0000-000000000090");

            if (!await db.Invoices.AnyAsync(x => x.Id == invoiceId))
            {
                var charges = await db.ShipmentCharges
                    .Where(x =>
                        x.Id == new Guid("00000000-0000-0000-0000-000000000081") ||
                        x.Id == new Guid("00000000-0000-0000-0000-000000000082"))
                    .ToListAsync();

                var invoice = new Invoice
                {
                    Id = invoiceId,
                    ShipmentId = shipmentId,

                    InvoiceNumber = "INV-2026-0001",

                    Currency = "USD",

                    SubTotal = 1650.00m,
                    TaxAmount = 0,
                    TotalAmount = 1650.00m,

                    PaymentStatus = PaymentStatus.Pending,

                    IssuedAt = DateTimeOffset.UtcNow,
                    DueDate = DateTimeOffset.UtcNow.AddDays(14),

                    PayerType = PayerType.Shipper,

                    CreatedAt = DateTimeOffset.UtcNow,
                    IsDeleted = false
                };

                foreach (var charge in charges)
                {
                    charge.InvoiceId = invoice.Id;
                    invoice.Charges.Add(charge);
                }

                db.Invoices.Add(invoice);

                await db.SaveChangesAsync();
            }
        }
    }
}