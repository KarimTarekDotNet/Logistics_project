using API.Extensions;
using Serilog;

namespace API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            // 1. Setup the initial bootstrap logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            Log.Information("Starting web application up...");

            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog((context, services, lc) =>
            {
                lc.ReadFrom.Configuration(context.Configuration)
                  .ReadFrom.Services(services)
                  .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information);
            });


            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.AddCorsConfiguration(builder.Configuration);

            builder.Services.AddRateLimitConfiguration();

            builder.Services.AddAuthConfiguration(builder.Configuration);

            var app = builder.Build();

            await app.SeedDatabaseAsync();

            app.UseApplicationMiddlewares(builder.Configuration);
            app.Run();
        }
    }
}
