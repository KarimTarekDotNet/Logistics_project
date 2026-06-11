using API.Mapping;
using Application.Interfaces.Repositories.Aliases;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Payments;
using Application.Interfaces.Repositories.Pricing.Imports;
using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.Shipments.Core;
using Application.Interfaces.Repositories.Shipments.User;
using Application.Interfaces.Repositories.ShippingCore;
using Application.Interfaces.Repositories.Users;
using Application.Interfaces.Services.Aliases;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Payment;
using Application.Interfaces.Services.Pricing.Imports;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Interfaces.Services.Shipments.ApisIntegrations;
using Application.Interfaces.Services.Shipments.Core;
using Application.Interfaces.Services.Shipments.User;
using Application.Interfaces.Services.System;
using Application.Interfaces.Services.User;
using Application.Validations.PricingFeature.Pricing;
using Domain.Entities.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data.Database;
using Infrastructure.Repositories.Aliases;
using Infrastructure.Repositories.Patterns;
using Infrastructure.Repositories.Payment;
using Infrastructure.Repositories.Pricing.Imports;
using Infrastructure.Repositories.Pricing.PricingEngine;
using Infrastructure.Repositories.Pricing.Quotation;
using Infrastructure.Repositories.Pricing.ShippingCore;
using Infrastructure.Repositories.Shipments;
using Infrastructure.Repositories.Shipments.Core;
using Infrastructure.Repositories.Users;
using Infrastructure.Services.Aliases;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Payment;
using Infrastructure.Services.Pricing.Imports;
using Infrastructure.Services.Pricing.PricingEngine;
using Infrastructure.Services.Pricing.Quotation;
using Infrastructure.Services.Pricing.ShippingCore;
using Infrastructure.Services.Shipments.Apis;
using Infrastructure.Services.Shipments.Core;
using Infrastructure.Services.Shipments.Core.Shipment;
using Infrastructure.Services.Shipments.User;
using Infrastructure.Services.System;
using Infrastructure.Services.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace API.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // cache
            var redisConnectionString =configuration.GetConnectionString("Redis");

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(redisConnectionString!)
            ?? throw new InvalidOperationException("Failed to connect to Redis"));


            // Repositories
            services.AddScoped<ICarrierRepository, CarrierRepository>();
            services.AddScoped<IContainerTypeRepository, ContainerTypeRepository>();
            services.AddScoped<IPortRepository, PortRepository>();
            services.AddScoped<IRouteRepository, RouteRepository>();
            services.AddScoped<IRateRepository, RateRepository>();
            services.AddScoped<IQuoteRepository, QuoteRepository>();
            services.AddScoped<ICustomerRepository, CustomerRepository>();
            services.AddScoped<IShipmentRepository, ShipmentRepository>();
            services.AddScoped<IShipmentItemRepository, ShipmentItemRepository>();
            services.AddScoped<IShipmentChargeRepository, ShipmentChargeRepository>();
            services.AddScoped<IShipmentStatusHistoryRepository, ShipmentStatusHistoryRepository>();
            services.AddScoped<IIntegrationMessageRepository, IntegrationMessageRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IInvoicePaymentRepository, InvoicePaymentRepository>();
            services.AddScoped<IShipmentDocumentRepository, ShipmentDocumentRepository>();
            services.AddScoped<IAliasRepository, AliasRepository>();
            services.AddScoped<IQuoteRequestRepository, QuoteRequestRepository>();
            services.AddScoped<IShipmentChargeRuleRepository, ShipmentChargeRuleRepository>();
            services.AddScoped<IPaymentTransactionRepository, PaymentTransactionRepository>();
            services.AddScoped<ISubscriptionPlanRepository, SubscriptionPlanRepository>();
            services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();

            // Unit of Work
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Services
            services.AddScoped<IRateService, RateService>();
            services.AddScoped<ICarrierService, CarrierService>();
            services.AddScoped<IContainerTypeService, ContainerTypeService>();
            services.AddScoped<IPortService, PortService>();
            services.AddScoped<IRouteService, RouteService>();
            services.AddScoped<IQuoteService, QuoteService>();
            services.AddScoped<IRateImportService, RateImportService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IShipmentQueryService, ShipmentQueryService>();
            services.AddScoped<IShipmentCommandService, ShipmentCommandService>();
            services.AddScoped<IShipmentLifecycleService, ShipmentLifecycleService>();
            services.AddScoped<IShipmentHoldService, ShipmentHoldService>();
            services.AddScoped<IShipmentCancellationService, ShipmentCancellationService>();
            services.AddScoped<IShipmentTrackingService, ShipmentTrackingService>();
            services.AddScoped<IShipmentItemService, ShipmentItemService>();
            services.AddScoped<IShipmentChargeService, ShipmentChargeService>();
            services.AddScoped<IShipmentStatusHistoryService, ShipmentStatusHistoryService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IInvoicePaymentService, InvoicePaymentService>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IShipmentTimelineService, ShipmentTimelineService>();
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IFileSecurityService, ClamAvFileScanner>();
            services.AddScoped<IShipmentDocumentService, ShipmentDocumentService>();
            services.AddScoped<IAliasService, AliasService>();
            services.AddScoped<IQuoteRequestService, QuoteRequestService>();
            services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
            services.AddScoped<IPaymobPaymentService, PaymobPaymentService>();
            services.AddScoped<IUserSubscriptionService, UserSubscriptionService>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddScoped<IRedisService, RedisService>();

            // APIs Integrations
            services.AddHttpClient<ITaxVerificationService, LookuptaxService>(client =>
            { client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("TaxVerification:TimeoutInSeconds")); });

            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IPhoneOtpService, TwilioPhoneOtpService>();

            // FluentValidation
            services.AddFluentValidationAutoValidation();
            services.AddValidatorsFromAssemblyContaining<CreateRateRequestValidator>();

            services.AddSwaggerGen();
            // AutoMapper
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            services.AddControllers();
            services.AddOpenApi();

            return services;
        }
    }
}
