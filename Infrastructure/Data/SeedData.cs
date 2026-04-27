using Domain.Entities.Pricing.PricingEngine;
using Domain.Entities.Pricing.Quotation;
using Domain.Entities.ShippingCore;

namespace Infrastructure.Data
{
    public static class SeedData
    {
        // Common timestamps
        private static readonly DateTimeOffset SeedCreatedAt =
            new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

        private static readonly DateTimeOffset QuoteCreatedAt =
            new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero);

        // ── Ports ────────────────────────────────────────────────────────────
        public static readonly Port PortShanghai = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000001"),
            Name = "Shanghai",
            Code = "CNSHA",
            Country = "China",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        public static readonly Port PortRotterdam = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000002"),
            Name = "Rotterdam",
            Code = "NLRTM",
            Country = "Netherlands",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        public static readonly Port PortDubai = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000003"),
            Name = "Dubai",
            Code = "AEJEA",
            Country = "UAE",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        // ── Carriers ─────────────────────────────────────────────────────────
        public static readonly Carrier CarrierMaersk = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000010"),
            Name = "Maersk Line",
            Code = "MAEU",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        public static readonly Carrier CarrierMSC = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000011"),
            Name = "Mediterranean Shipping Company",
            Code = "MSCU",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        // ── Container Types ──────────────────────────────────────────────────
        public static readonly ContainerType Container20Ft = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000020"),
            Name = "20ft Standard",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        public static readonly ContainerType Container40Ft = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000021"),
            Name = "40ft Standard",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        public static readonly ContainerType Container40HQ = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000022"),
            Name = "40ft High Cube",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        // ── Routes ───────────────────────────────────────────────────────────
        public static readonly Route RouteShanghaiToRotterdam = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000030"),
            FromPortId = PortShanghai.Id,
            ToPortId = PortRotterdam.Id,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        public static readonly Route RouteShanghaiToDubai = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000031"),
            FromPortId = PortShanghai.Id,
            ToPortId = PortDubai.Id,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = SeedCreatedAt,
            UpdatedAt = null
        };

        // ── Rates ────────────────────────────────────────────────────────────
        public static readonly Rate RateMaerskShanghaiRotterdam20Ft = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000040"),
            CarrierId = CarrierMaersk.Id,
            RouteId = RouteShanghaiToRotterdam.Id,
            ContainerTypeId = Container20Ft.Id,
            Price = 1500.00m,
            Currency = "USD",
            ValidFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidTo = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero),
            DeletedAt = null,
            IsDeleted = false,
            IsActive = true
        };

        public static readonly Rate RateMaerskShanghaiRotterdam40Ft = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000041"),
            CarrierId = CarrierMaersk.Id,
            RouteId = RouteShanghaiToRotterdam.Id,
            ContainerTypeId = Container40Ft.Id,
            Price = 2800.00m,
            Currency = "USD",
            ValidFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidTo = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2025, 1, 5, 0, 0, 0, TimeSpan.Zero),
            DeletedAt = null,
            IsDeleted = false,
            IsActive = false
        };

        public static readonly Rate RateMSCShanghaiDubai20Ft = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000042"),
            CarrierId = CarrierMSC.Id,
            RouteId = RouteShanghaiToDubai.Id,
            ContainerTypeId = Container20Ft.Id,
            Price = 900.00m,
            Currency = "USD",
            ValidFrom = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ValidTo = new DateTimeOffset(2025, 12, 31, 0, 0, 0, TimeSpan.Zero),
            CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null,
            DeletedAt = null,
            IsDeleted = false,
            IsActive = true
        };

        // ── Quotes ───────────────────────────────────────────────────────────
        public static readonly Quote QuoteAlpha = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000060"),
            CustomerName = "Alpha Trading Co.",
            RouteId = RouteShanghaiToRotterdam.Id,
            ContainerTypeId = Container20Ft.Id,
            FinalPrice = 1650.00m,
            Currency = "USD",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null
        };

        public static readonly Quote QuoteBeta = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000061"),
            CustomerName = "Beta Logistics Ltd.",
            RouteId = RouteShanghaiToDubai.Id,
            ContainerTypeId = Container40Ft.Id,
            FinalPrice = 3100.00m,
            Currency = "USD",
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null
        };

        // ── Quote Items ──────────────────────────────────────────────────────
        public static readonly QuoteItem QuoteAlphaFreight = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000070"),
            QuoteId = QuoteAlpha.Id,
            Description = "Ocean Freight",
            Amount = 1500.00m,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null
        };

        public static readonly QuoteItem QuoteAlphaSurcharge = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000071"),
            QuoteId = QuoteAlpha.Id,
            Description = "Bunker Adjustment Factor",
            Amount = 150.00m,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = new DateTimeOffset(2025, 2, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null
        };

        public static readonly QuoteItem QuoteBetaFreight = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000072"),
            QuoteId = QuoteBeta.Id,
            Description = "Ocean Freight",
            Amount = 2800.00m,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null
        };

        public static readonly QuoteItem QuoteBetaSurcharge = new()
        {
            Id = new Guid("00000000-0000-0000-0000-000000000073"),
            QuoteId = QuoteBeta.Id,
            Description = "Port Handling Fee",
            Amount = 300.00m,
            IsDeleted = false,
            DeletedAt = null,
            CreatedAt = new DateTimeOffset(2025, 2, 15, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = null
        };
    }
}