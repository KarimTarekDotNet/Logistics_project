using API.Mapping;
using API.Middlewares;
using Application.Interfaces.Repositories.Patterns;
using Application.Interfaces.Repositories.Pricing.PricingEngine;
using Application.Interfaces.Repositories.Pricing.Quotation;
using Application.Interfaces.Repositories.Shipments.Core;
using Application.Interfaces.Repositories.Shipments.User;
using Application.Interfaces.Repositories.ShippingCore;
using Application.Interfaces.Services.Auth;
using Application.Interfaces.Services.Pricing.Imports;
using Application.Interfaces.Services.Pricing.PricingEngine;
using Application.Interfaces.Services.Pricing.Quotation;
using Application.Interfaces.Services.Pricing.ShippingCore;
using Application.Interfaces.Services.Shipments.ApisIntegrations;
using Application.Interfaces.Services.Shipments.Core;
using Application.Interfaces.Services.Shipments.User;
using Application.Validations.PricingFeature.Pricing;
using Domain.Entities.Users;
using FluentValidation;
using FluentValidation.AspNetCore;
using Infrastructure.Data.Configuration.Seeding;
using Infrastructure.Data.Database;
using Infrastructure.Repositories.Patterns;
using Infrastructure.Repositories.Pricing.PricingEngine;
using Infrastructure.Repositories.Pricing.Quotation;
using Infrastructure.Repositories.Pricing.ShippingCore;
using Infrastructure.Repositories.Shipments;
using Infrastructure.Repositories.Shipments.Core;
using Infrastructure.Services.Auth;
using Infrastructure.Services.Pricing.Imports;
using Infrastructure.Services.Pricing.PricingEngine;
using Infrastructure.Services.Pricing.Quotation;
using Infrastructure.Services.Pricing.ShippingCore;
using Infrastructure.Services.Shipments.Apis;
using Infrastructure.Services.Shipments.Core;
using Infrastructure.Services.Shipments.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Database
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services
                .AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

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
            builder.Services.AddScoped<IShipmentService, ShipmentService>();
            builder.Services.AddScoped<IShipmentItemService, ShipmentItemService>();
            builder.Services.AddScoped<IShipmentChargeService, ShipmentChargeService>();
            builder.Services.AddScoped<IShipmentStatusHistoryService, ShipmentStatusHistoryService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer("Bearer", options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
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

                options.MapInboundClaims = false;
            });

            // APIs Integrations
            builder.Services.AddHttpClient<ITaxVerificationService, LookuptaxService>();
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

            //using (var scope = app.Services.CreateScope())
            //{
            //    var services = scope.ServiceProvider;

            //    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            //    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            //    var dbContext = services.GetRequiredService<ApplicationDbContext>();

            //    await AppSeeder.SeedAsync(roleManager, userManager, dbContext);
            //}

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.MapOpenApi();
                app.UseSwaggerUI();
            }

            app.UseAuthentication();

            app.UseMiddleware<GlobalHandleExceptionMiddleware>();

            app.UseRateLimiter();

            app.UseAuthorization();

            app.UseHttpsRedirection();

            app.MapControllers();
            app.Run();
        }
    }
}
