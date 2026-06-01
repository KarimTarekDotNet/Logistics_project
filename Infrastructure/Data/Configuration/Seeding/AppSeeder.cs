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
        public static async Task SeedAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db)
        {
            await SeedRolesAsync(roleManager);
            var customerUser = await SeedUsersAsync(userManager);
            await SeedContainerTypesAsync(db);
            await SeedPortsAsync(db);
            await SeedCarriersAsync(db);
            await SeedRoutesAsync(db);
            await SeedRatesAsync(db);
            await SeedShipmentChargeRulesAsync(db);
            var customer = await SeedCustomerAsync(db, customerUser);
            await SeedQuotesAsync(db, customer);
            await SeedShipmentAsync(db, customer);
            await SeedInvoiceAsync(db);
        }

        // =========================
        // 1. Roles
        // =========================
        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            foreach (var role in Enum.GetValues<Role>())
            {
                var roleName = role.ToString();
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // =========================
        // 2. Users
        // =========================
        private static async Task<ApplicationUser> SeedUsersAsync(UserManager<ApplicationUser> userManager)
        {
            await CreateUserIfNotExists(userManager, "admin@system.com",       "admin@system",       "System",      "Admin",       "Admin");
            await CreateUserIfNotExists(userManager, "staff@system.com",       "staff@system",       "Staff",       "Staff",       "Staff");
            await CreateUserIfNotExists(userManager, "integration@system.com", "integration@system", "Integration", "System",      "Integration");

            return await CreateUserIfNotExists(userManager, "user@system.com", "user@system", "System", "User", "User");
        }

        private static async Task<ApplicationUser> CreateUserIfNotExists(
            UserManager<ApplicationUser> userManager,
            string email, string username, string firstName, string lastName, string role)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null) return user;

            user = new ApplicationUser
            {
                UserName       = username,
                Email          = email,
                FirstName      = firstName,
                LastName       = lastName,
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, $"{role}@123");
            await userManager.AddToRoleAsync(user, role);
            return user;
        }

        // =========================
        // 3. Container Types
        // =========================
        //
        // GUID range: 00000000-0000-0000-0000-0000000000xx (20–2B)
        //
        // Refrigerated containers (Reefer) support temperature ranges.
        // Rate3 (MSC, Shanghai→Dubai) has MinTemperatureCelsius = -20,
        // so it must be linked to a reefer container — see SeedRatesAsync.
        //
        // NOTE: ContainerType entity only has { Id, Name, IsDeleted, CreatedAt, UpdatedAt }.
        //       If you later add fields like MaxGrossWeightKg / MaxVolumeCbm to the entity,
        //       update the rows below accordingly.
        // =========================
        public static readonly Guid Container20FtDryId        = new("00000000-0000-0000-0000-000000000020");
        public static readonly Guid Container40FtDryId        = new("00000000-0000-0000-0000-000000000021");
        public static readonly Guid Container40HqId           = new("00000000-0000-0000-0000-000000000022");
        public static readonly Guid Container20FtReeferId     = new("00000000-0000-0000-0000-000000000023");
        public static readonly Guid Container40FtReeferId     = new("00000000-0000-0000-0000-000000000024");
        public static readonly Guid Container20FtOpenTopId    = new("00000000-0000-0000-0000-000000000025");
        public static readonly Guid Container40FtOpenTopId    = new("00000000-0000-0000-0000-000000000026");
        public static readonly Guid Container20FtFlatRackId   = new("00000000-0000-0000-0000-000000000027");
        public static readonly Guid Container40FtFlatRackId   = new("00000000-0000-0000-0000-000000000028");
        public static readonly Guid Container20FtTankId       = new("00000000-0000-0000-0000-000000000029");
        public static readonly Guid Container45HqId           = new("00000000-0000-0000-0000-00000000002A");
        public static readonly Guid ContainerLclId            = new("00000000-0000-0000-0000-00000000002B");

        private static async Task SeedContainerTypesAsync(ApplicationDbContext db)
        {
            var seedDate = SeedDate;

            var containers = new List<ContainerType>
            {
                new() { Id = Container20FtDryId,      Name = "20ft Dry Standard",    IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container40FtDryId,      Name = "40ft Dry Standard",    IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container40HqId,         Name = "40ft High Cube",       IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container20FtReeferId,   Name = "20ft Reefer",          IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container40FtReeferId,   Name = "40ft Reefer",          IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container20FtOpenTopId,  Name = "20ft Open Top",        IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container40FtOpenTopId,  Name = "40ft Open Top",        IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container20FtFlatRackId, Name = "20ft Flat Rack",       IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container40FtFlatRackId, Name = "40ft Flat Rack",       IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container20FtTankId,     Name = "20ft Tank",            IsDeleted = false, CreatedAt = seedDate },
                new() { Id = Container45HqId,         Name = "45ft High Cube",       IsDeleted = false, CreatedAt = seedDate },
                new() { Id = ContainerLclId,          Name = "LCL / Loose Cargo",    IsDeleted = false, CreatedAt = seedDate },
            };

            var existingIds = await db.ContainerTypes
                .Select(c => c.Id)
                .ToListAsync();

            var toInsert = containers
                .Where(c => !existingIds.Contains(c.Id))
                .ToList();

            if (toInsert.Count > 0)
            {
                await db.ContainerTypes.AddRangeAsync(toInsert);
                await db.SaveChangesAsync();
            }
        }

        // =========================
        // 4. Ports
        // =========================
        private static readonly Guid PortShanghaiId   = new("00000000-0000-0000-0000-000000000001");
        private static readonly Guid PortRotterdamId  = new("00000000-0000-0000-0000-000000000002");
        private static readonly Guid PortDubaiId      = new("00000000-0000-0000-0000-000000000003");

        private static async Task SeedPortsAsync(ApplicationDbContext db)
        {
            var seedDate = SeedDate;

            var ports = new List<Port>
            {
                new() { Id = PortShanghaiId,  Name = "Shanghai",   Code = "CNSHA", Country = "China",       IsDeleted = false, CreatedAt = seedDate },
                new() { Id = PortRotterdamId, Name = "Rotterdam",  Code = "NLRTM", Country = "Netherlands", IsDeleted = false, CreatedAt = seedDate },
                new() { Id = PortDubaiId,     Name = "Dubai",      Code = "AEJEA", Country = "UAE",         IsDeleted = false, CreatedAt = seedDate },
            };

            var existingIds = await db.Ports.Select(p => p.Id).ToListAsync();
            var toInsert = ports.Where(p => !existingIds.Contains(p.Id)).ToList();

            if (toInsert.Count > 0)
            {
                await db.Ports.AddRangeAsync(toInsert);
                await db.SaveChangesAsync();
            }
        }

        // =========================
        // 5. Carriers
        // =========================
        private static readonly Guid CarrierMaerskId = new("00000000-0000-0000-0000-000000000010");
        private static readonly Guid CarrierMscId    = new("00000000-0000-0000-0000-000000000011");

        private static async Task SeedCarriersAsync(ApplicationDbContext db)
        {
            var seedDate = SeedDate;

            var carriers = new List<Carrier>
            {
                new() { Id = CarrierMaerskId, Name = "Maersk Line",                    Code = "MAEU", IsDeleted = false, CreatedAt = seedDate },
                new() { Id = CarrierMscId,    Name = "Mediterranean Shipping Company", Code = "MSCU", IsDeleted = false, CreatedAt = seedDate },
            };

            var existingIds = await db.Carriers.Select(c => c.Id).ToListAsync();
            var toInsert = carriers.Where(c => !existingIds.Contains(c.Id)).ToList();

            if (toInsert.Count > 0)
            {
                await db.Carriers.AddRangeAsync(toInsert);
                await db.SaveChangesAsync();
            }
        }

        // =========================
        // 6. Routes
        // =========================
        private static readonly Guid RouteShanghaiRotterdamId = new("00000000-0000-0000-0000-000000000030");
        private static readonly Guid RouteShanghaiDubaiId     = new("00000000-0000-0000-0000-000000000031");

        private static async Task SeedRoutesAsync(ApplicationDbContext db)
        {
            var seedDate = SeedDate;

            var routes = new List<Route>
            {
                new() { Id = RouteShanghaiRotterdamId, FromPortId = PortShanghaiId, ToPortId = PortRotterdamId, IsDeleted = false, CreatedAt = seedDate },
                new() { Id = RouteShanghaiDubaiId,     FromPortId = PortShanghaiId, ToPortId = PortDubaiId,     IsDeleted = false, CreatedAt = seedDate },
            };

            var existingIds = await db.Routes.Select(r => r.Id).ToListAsync();
            var toInsert = routes.Where(r => !existingIds.Contains(r.Id)).ToList();

            if (toInsert.Count > 0)
            {
                await db.Routes.AddRangeAsync(toInsert);
                await db.SaveChangesAsync();
            }
        }

        // =========================
        // 7. Rates
        // =========================
        // Rate1 : Maersk | Shanghai→Rotterdam | 20ft Dry     | no temp
        // Rate2 : Maersk | Shanghai→Rotterdam | 40ft Dry     | no temp
        // Rate3 : MSC    | Shanghai→Dubai     | 20ft Reefer  | -20°C → +25°C  ← reefer container
        // =========================
        private static readonly Guid Rate1Id = new("00000000-0000-0000-0000-000000000040");
        private static readonly Guid Rate2Id = new("00000000-0000-0000-0000-000000000041");
        private static readonly Guid Rate3Id = new("00000000-0000-0000-0000-000000000042");

        private static async Task SeedRatesAsync(ApplicationDbContext db)
        {
            var seedDate    = SeedDate;
            var rateUpdated = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero);
            var validFrom   = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var validTo     = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero);

            var rates = new List<Rate>
            {
                new()
                {
                    Id              = Rate1Id,
                    CarrierId       = CarrierMaerskId,
                    RouteId         = RouteShanghaiRotterdamId,
                    ContainerTypeId = Container20FtDryId,       // 20ft Dry
                    Price           = 1500.00m,
                    Currency        = "USD",
                    ValidFrom       = validFrom,
                    ValidTo         = validTo,
                    MaxGrossWeightKg      = 20000m,
                    MaxNetWeightKg        = 18000m,
                    MaxVolumeCbm          = 28m,
                    AllowsHazardous       = false,
                    MinTemperatureCelsius = null,
                    MaxTemperatureCelsius = null,
                    IsActive    = true,
                    IsDeleted   = false,
                    CreatedAt   = seedDate,
                    UpdatedAt   = rateUpdated
                },
                new()
                {
                    Id              = Rate2Id,
                    CarrierId       = CarrierMaerskId,
                    RouteId         = RouteShanghaiRotterdamId,
                    ContainerTypeId = Container40FtDryId,       // 40ft Dry
                    Price           = 2800.00m,
                    Currency        = "USD",
                    ValidFrom       = validFrom,
                    ValidTo         = validTo,
                    MaxGrossWeightKg      = 28000m,
                    MaxNetWeightKg        = 26000m,
                    MaxVolumeCbm          = 58m,
                    AllowsHazardous       = false,
                    MinTemperatureCelsius = null,
                    MaxTemperatureCelsius = null,
                    IsActive    = false,
                    IsDeleted   = false,
                    CreatedAt   = seedDate,
                    UpdatedAt   = rateUpdated
                },
                new()
                {
                    Id              = Rate3Id,
                    CarrierId       = CarrierMscId,
                    RouteId         = RouteShanghaiDubaiId,
                    ContainerTypeId = Container20FtReeferId,    // 20ft Reefer — supports -20°C
                    Price           = 900.00m,
                    Currency        = "USD",
                    ValidFrom       = validFrom,
                    ValidTo         = validTo,
                    MaxGrossWeightKg      = 20000m,
                    MaxNetWeightKg        = 18000m,
                    MaxVolumeCbm          = 28m,
                    AllowsHazardous       = true,
                    MinTemperatureCelsius = -20m,
                    MaxTemperatureCelsius = 25m,
                    IsActive    = true,
                    IsDeleted   = false,
                    CreatedAt   = seedDate
                },
            };

            var existingIds = await db.Rates.Select(r => r.Id).ToListAsync();
            var toInsert = rates.Where(r => !existingIds.Contains(r.Id)).ToList();

            if (toInsert.Count > 0)
            {
                await db.Rates.AddRangeAsync(toInsert);
                await db.SaveChangesAsync();
            }
        }

        // =========================
        // 8. Shipment Charge Rules
        // =========================
        // Calculation logic (from service):
        //   Fixed                  => rule.Value
        //   PerKg                  => TotalChargeableWeightKg * rule.Value
        //   PerCbm                 => TotalVolumeCbm * rule.Value
        //   PercentageOfAgreedPrice => AgreedPrice * rule.Value / 100
        //
        // Values are chosen so a typical shipment (18 000 kg, 25 cbm, $1 650 agreed)
        // produces realistic charge amounts.
        // =========================
        private static async Task SeedShipmentChargeRulesAsync(ApplicationDbContext db)
        {
            if (await db.ShipmentChargeRules.AnyAsync())
                return;

            var rules = new List<ShipmentChargeRule>
            {
                // Ocean Freight — 100 % of agreed price (the base freight cost)
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A1"),
                    ChargeType      = ChargeType.OceanFreight,
                    PayerType       = PayerType.Shipper,
                    CalculationType = ChargeCalculationType.PercentageOfAgreedPrice,
                    Value           = 100m,   // 100 % → equals the agreed price
                    Currency        = "USD",
                    IsActive        = true
                },

                // Documentation — fixed fee per shipment
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A2"),
                    ChargeType      = ChargeType.Documentation,
                    PayerType       = PayerType.Shipper,
                    CalculationType = ChargeCalculationType.Fixed,
                    Value           = 75m,    // $75 flat
                    Currency        = "USD",
                    IsActive        = true
                },

                // Handling — per CBM (port handling at origin)
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A3"),
                    ChargeType      = ChargeType.Handling,
                    PayerType       = PayerType.Shipper,
                    CalculationType = ChargeCalculationType.PerCbm,
                    Value           = 4m,     // $4 / CBM → 25 cbm = $100
                    Currency        = "USD",
                    IsActive        = true
                },

                // Customs — fixed fee (destination customs clearance)
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A4"),
                    ChargeType      = ChargeType.Customs,
                    PayerType       = PayerType.Consignee,
                    CalculationType = ChargeCalculationType.Fixed,
                    Value           = 150m,   // $150 flat
                    Currency        = "USD",
                    IsActive        = true
                },

                // Insurance — percentage of agreed price
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A5"),
                    ChargeType      = ChargeType.Insurance,
                    PayerType       = PayerType.Shipper,
                    CalculationType = ChargeCalculationType.PercentageOfAgreedPrice,
                    Value           = 0.5m,   // 0.5 % → $1 650 * 0.5 / 100 = $8.25
                    Currency        = "USD",
                    IsActive        = true
                },

                // Storage — per CBM per day (billed when cargo is held at port)
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A6"),
                    ChargeType      = ChargeType.Storage,
                    PayerType       = PayerType.Consignee,
                    CalculationType = ChargeCalculationType.PerCbm,
                    Value           = 2m,     // $2 / CBM
                    Currency        = "USD",
                    IsActive        = true
                },

                // Other — fixed miscellaneous fee (inactive by default)
                new()
                {
                    Id              = new Guid("00000000-0000-0000-0000-0000000000A7"),
                    ChargeType      = ChargeType.Other,
                    PayerType       = PayerType.Shipper,
                    CalculationType = ChargeCalculationType.Fixed,
                    Value           = 50m,    // $50 flat
                    Currency        = "USD",
                    IsActive        = false   // inactive — applied only when explicitly needed
                },
            };

            await db.ShipmentChargeRules.AddRangeAsync(rules);
            await db.SaveChangesAsync();
        }

        // =========================
        // 9. Customer
        // =========================
        private static async Task<Customer> SeedCustomerAsync(ApplicationDbContext db, ApplicationUser customerUser)
        {
            var customer = await db.Customers.FirstOrDefaultAsync(x => x.ApplicationUserId == customerUser.Id);
            if (customer != null) return customer;

            customer = new Customer
            {
                Id                = new Guid("00000000-0000-0000-0000-0000000000C1"),
                ApplicationUserId = customerUser.Id,
                NationalId        = "29801011234567",
                CompanyName       = "Acme Corporation",
                TaxNumber         = "TAX123456789",
                DateOfBirth       = new DateOnly(1990, 1, 1),
                CreatedAt         = SeedDate,
                IsDeleted         = false
            };

            db.Customers.Add(customer);
            await db.SaveChangesAsync();
            return customer;
        }

        // =========================
        // 10. Quotes
        // =========================
        // quoteAlpha → 20ft Dry  (Rate1, Maersk, Shanghai→Rotterdam) — Accepted
        // quoteBeta  → 40ft Dry  (Rate2, Maersk, Shanghai→Rotterdam) — Pending
        // =========================
        private static readonly Guid QuoteAlphaId = new("00000000-0000-0000-0000-000000000060");
        private static readonly Guid QuoteBetaId  = new("00000000-0000-0000-0000-000000000061");

        private static async Task SeedQuotesAsync(ApplicationDbContext db, Customer customer)
        {
            var quotes = new List<Quote>();

            if (!await db.Quotes.AnyAsync(q => q.Id == QuoteAlphaId))
            {
                quotes.Add(new Quote
                {
                    Id              = QuoteAlphaId,
                    CustomerId      = customer.Id,
                    CarrierId       = CarrierMaerskId,
                    RateId          = Rate1Id,
                    RouteId         = RouteShanghaiRotterdamId,
                    ContainerTypeId = Container20FtDryId,
                    FinalPrice      = 1650.00m,
                    Currency        = "USD",
                    RequestedGrossWeightKg      = 18000m,
                    RequestedNetWeightKg        = 16000m,
                    RequestedVolumeCbm          = 25m,
                    RequestedChargeableWeightKg = 18000m,
                    IsHazardous     = false,
                    Status          = QuoteStatus.Accepted,
                    IsDeleted       = false,
                    CreatedAt       = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
                });
            }

            if (!await db.Quotes.AnyAsync(q => q.Id == QuoteBetaId))
            {
                quotes.Add(new Quote
                {
                    Id              = QuoteBetaId,
                    CustomerId      = customer.Id,
                    CarrierId       = CarrierMaerskId,
                    RateId          = Rate2Id,
                    RouteId         = RouteShanghaiRotterdamId,
                    ContainerTypeId = Container40FtDryId,
                    FinalPrice      = 3100.00m,
                    Currency        = "USD",
                    RequestedGrossWeightKg      = 26000m,
                    RequestedNetWeightKg        = 24000m,
                    RequestedVolumeCbm          = 55m,
                    RequestedChargeableWeightKg = 26000m,
                    IsHazardous     = false,
                    Status          = QuoteStatus.Pending,
                    IsDeleted       = false,
                    CreatedAt       = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero)
                });
            }

            if (quotes.Count > 0)
            {
                await db.Quotes.AddRangeAsync(quotes);
                await db.SaveChangesAsync();
            }
        }

        // =========================
        // 11. Shipment
        // =========================
        private static readonly Guid ShipmentId = new("00000000-0000-0000-0000-000000000080");

        private static async Task SeedShipmentAsync(ApplicationDbContext db, Customer customer)
        {
            var exists = await db.Shipments.AnyAsync(x => x.Id == ShipmentId);
            if (exists) return;

            var shipment = new Shipment
            {
                Id              = ShipmentId,
                QuoteId         = QuoteAlphaId,
                CustomerId      = customer.Id,
                RouteId         = RouteShanghaiRotterdamId,
                ContainerTypeId = Container20FtDryId,
                CarrierId       = CarrierMaerskId,

                AllowedGrossWeightKg      = 20000m,
                AllowedNetWeightKg        = 18000m,
                AllowedVolumeCbm          = 28m,
                AllowedChargeableWeightKg = 20000m,
                IsHazardousAllowed        = false,

                TotalGrossWeightKg      = 18000m,
                TotalNetWeightKg        = 16000m,
                TotalVolumeCbm          = 25m,
                TotalChargeableWeightKg = 18000m,

                AgreedPrice = 1650.00m,
                Currency    = "USD",
                Status      = ShipmentStatus.ClientConfirmed,
                CreatedAt   = SeedDate,
                ClientConfirmedAt = SeedDate,
                IsDeleted   = false
            };

            shipment.Items.Add(new ShipmentItem
            {
                Id          = new Guid("00000000-0000-0000-0000-000000000083"),
                Description = "Textile Goods",
                Quantity    = 20,
                GrossWeight = 18000m,
                NetWeight   = 16000m,
                VolumeCbm   = 25m,
                ChargeableWeight = 18000m,
                IsHazardous = false,
                CreatedAt   = SeedDate,
                IsDeleted   = false
            });

            shipment.Charges.Add(new ShipmentCharge
            {
                Id          = new Guid("00000000-0000-0000-0000-000000000081"),
                Description = "Ocean Freight",
                Amount      = 1500m,
                TaxAmount   = 0.14m * 1500m,
                Currency    = "USD",
                ChargeType  = ChargeType.OceanFreight,
                PayerType   = PayerType.Shipper,
                CreatedAt   = SeedDate,
                IsDeleted   = false
            });

            shipment.Charges.Add(new ShipmentCharge
            {
                Id          = new Guid("00000000-0000-0000-0000-000000000082"),
                Description = "Bunker Adjustment Factor",
                Amount      = 150m,
                TaxAmount   = 0.14m * 150m,
                Currency    = "USD",
                ChargeType  = ChargeType.Other,
                PayerType   = PayerType.Shipper,
                CreatedAt   = SeedDate,
                IsDeleted   = false
            });

            shipment.StatusHistory.Add(new ShipmentStatusHistory
            {
                Id          = new Guid("00000000-0000-0000-0000-000000000084"),
                FromStatus  = ShipmentStatus.Created,
                ToStatus    = ShipmentStatus.ClientConfirmed,
                ChangedAt   = SeedDate,
                ChangedBy   = "System",
                Reason      = "Client confirmed quote"
            });

            db.Shipments.Add(shipment);
            await db.SaveChangesAsync();
        }

        // =========================
        // 12. Invoice
        // =========================
        private static readonly Guid InvoiceId = new("00000000-0000-0000-0000-000000000090");

        private static async Task SeedInvoiceAsync(ApplicationDbContext db)
        {
            if (await db.Invoices.AnyAsync(x => x.Id == InvoiceId))
                return;

            var charges = await db.ShipmentCharges
                .Where(x => x.Id == new Guid("00000000-0000-0000-0000-000000000081") ||
                            x.Id == new Guid("00000000-0000-0000-0000-000000000082"))
                .ToListAsync();

            var invoice = new Invoice
            {
                Id            = InvoiceId,
                ShipmentId    = ShipmentId,
                InvoiceNumber = "INV-2025-0001",
                Currency      = "USD",
                SubTotal      = 1650.00m,
                NetShipmentPrice = 1650.00m,
                TaxAmount     = 0.14m * 1650.00m,
                TotalAmount   = 1650.00m + (0.14m * 1650.00m),
                PaymentStatus = PaymentStatus.Paid,
                IssuedAt      = SeedDate,
                DueDate       = SeedDate.AddDays(14),
                PayerType     = PayerType.Shipper,
                CreatedAt     = SeedDate,
                PaidAt        = SeedDate.AddDays(10),
                IsDeleted     = false
            };

            foreach (var charge in charges)
            {
                charge.InvoiceId = invoice.Id;
                invoice.Charges.Add(charge);
            }

            db.Invoices.Add(invoice);
            await db.SaveChangesAsync();
        }

        // =========================
        // Shared
        // =========================
        private static readonly DateTimeOffset SeedDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);
    }
}
