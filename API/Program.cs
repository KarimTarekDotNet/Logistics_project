#region Using namespaces

using API.Mapping;
using API.Middlewares;
using Application.Interfaces.Repositories.Aliases;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Pricing.Imports;
using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.Shipments.Core;
using Application.Interfaces.Repositories.Shipments.User;
using Application.Interfaces.Repositories.ShippingCore;
using Application.Interfaces.Services.Aliases;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Pricing.Imports;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Interfaces.Services.Shipments.ApisIntegrations;
using Application.Interfaces.Services.Shipments.Core;
using Application.Interfaces.Services.Shipments.User;
using Application.Interfaces.Services.User;
using Application.Validations.PricingFeature.Pricing;
using Domain.Entities.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data.Configuration.Seeding;
using Infrastructure.Data.Database;
using Infrastructure.Repositories.Aliases;
using Infrastructure.Repositories.Patterns;
using Infrastructure.Repositories.Pricing.Imports;
using Infrastructure.Repositories.Pricing.PricingEngine;
using Infrastructure.Repositories.Pricing.Quotation;
using Infrastructure.Repositories.Pricing.ShippingCore;
using Infrastructure.Repositories.Shipments;
using Infrastructure.Repositories.Shipments.Core;
using Infrastructure.Services.Aliases;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Pricing.Imports;
using Infrastructure.Services.Pricing.PricingEngine;
using Infrastructure.Services.Pricing.Quotation;
using Infrastructure.Services.Pricing.ShippingCore;
using Infrastructure.Services.Shipments.Apis;
using Infrastructure.Services.Shipments.Core;
using Infrastructure.Services.Shipments.Core.Shipment;
using Infrastructure.Services.Shipments.User;
using Infrastructure.Services.User;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

#endregion

namespace API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            const string FrontendCorsPolicy = "FrontendCors";

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(FrontendCorsPolicy, policy =>
                {
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://127.0.0.1:5173",
                            "https://localhost:5173",
                            "https://127.0.0.1:5173",
                            "https://karimtarekdotnet.github.io"
                        )
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            #region Rate Limiting

            builder.Services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("AuthPolicy", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("OtpPolicy", opt =>
                {
                    opt.PermitLimit = 3;
                    opt.Window = TimeSpan.FromMinutes(5);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("HeavyPolicy", opt =>
                {
                    opt.PermitLimit = 20;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("ReadPolicy", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueLimit = 0;
                });
            });

            #endregion

            #region Dependency Injection
            // Repositories
            builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();
            builder.Services.AddScoped<IContainerTypeRepository, ContainerTypeRepository>();
            builder.Services.AddScoped<IPortRepository, PortRepository>();
            builder.Services.AddScoped<IRouteRepository, RouteRepository>();
            builder.Services.AddScoped<IRateRepository, RateRepository>();
            builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IShipmentRepository, ShipmentRepository>();
            builder.Services.AddScoped<IShipmentItemRepository, ShipmentItemRepository>();
            builder.Services.AddScoped<IShipmentChargeRepository, ShipmentChargeRepository>();
            builder.Services.AddScoped<IShipmentStatusHistoryRepository, ShipmentStatusHistoryRepository>();
            builder.Services.AddScoped<IIntegrationMessageRepository, IntegrationMessageRepository>();
            builder.Services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            builder.Services.AddScoped<IShipmentDocumentRepository, ShipmentDocumentRepository>();
            builder.Services.AddScoped<IAliasRepository, AliasRepository>();
            builder.Services.AddScoped<IQuoteRequestRepository, QuoteRequestRepository>();
            builder.Services.AddScoped<IShipmentChargeRuleRepository, ShipmentChargeRuleRepository>();

            // Unit of Work
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

            // Services
            builder.Services.AddScoped<IRateService, RateService>();
            builder.Services.AddScoped<ICarrierService, CarrierService>();
            builder.Services.AddScoped<IContainerTypeService, ContainerTypeService>();
            builder.Services.AddScoped<IPortService, PortService>();
            builder.Services.AddScoped<IRouteService, RouteService>();
            builder.Services.AddScoped<IQuoteService, QuoteService>();
            builder.Services.AddScoped<IRateImportService, RateImportService>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IShipmentQueryService, ShipmentQueryService>();
            builder.Services.AddScoped<IShipmentCommandService, ShipmentCommandService>();
            builder.Services.AddScoped<IShipmentLifecycleService, ShipmentLifecycleService>();
            builder.Services.AddScoped<IShipmentHoldService, ShipmentHoldService>();
            builder.Services.AddScoped<IShipmentCancellationService, ShipmentCancellationService>();
            builder.Services.AddScoped<IShipmentTrackingService, ShipmentTrackingService>();
            builder.Services.AddScoped<IShipmentItemService, ShipmentItemService>();
            builder.Services.AddScoped<IShipmentChargeService, ShipmentChargeService>();
            builder.Services.AddScoped<IShipmentStatusHistoryService, ShipmentStatusHistoryService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IInvoiceService, InvoiceService>();
            builder.Services.AddScoped<IShipmentTimelineService, ShipmentTimelineService>();
            builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
            builder.Services.AddScoped<IFileSecurityService, ClamAvFileScanner>();
            builder.Services.AddScoped<IShipmentDocumentService, ShipmentDocumentService>();
            builder.Services.AddScoped<IAliasService, AliasService>();
            builder.Services.AddScoped<IQuoteRequestService, QuoteRequestService>();

            #endregion

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["AuthToken"];
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAntiforgery(options =>
            {
                options.HeaderName = "X-CSRF-TOKEN";
                options.Cookie.Name = "XSRF-TOKEN";
                options.Cookie.HttpOnly = false;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
            });

            // APIs Integrations
            builder.Services.AddHttpClient<ITaxVerificationService, LookuptaxService>(client => { client.Timeout = TimeSpan.FromSeconds(10);} );
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IPhoneOtpService, TwilioPhoneOtpService>();

            // FluentValidation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<CreateRateRequestValidator>();

            builder.Services.AddSwaggerGen();
            // AutoMapper
            builder.Services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                var dbContext = services.GetRequiredService<ApplicationDbContext>();

                await AppSeeder.SeedAsync(roleManager, userManager, dbContext);
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.MapOpenApi();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(FrontendCorsPolicy);

            app.UseStaticFiles();
            app.UseAuthentication();


            app.UseMiddleware<GlobalHandleExceptionMiddleware>();

            app.Use(async (context, next) =>
            {
                var method = context.Request.Method;

                if (context.Request.Cookies.ContainsKey("AuthToken") &&
                    method is "POST" or "PUT" or "PATCH" or "DELETE")
                {
                    var antiforgery = context.RequestServices.GetRequiredService<IAntiforgery>();
                    await antiforgery.ValidateRequestAsync(context);
                }

                await next();
            });

            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}
