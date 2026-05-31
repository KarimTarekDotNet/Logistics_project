using API.Extensions;

namespace API
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddApplicationServices(builder.Configuration);

            builder.Services.AddCorsConfiguration();

            builder.Services.AddRateLimitConfiguration();

            builder.Services.AddAuthConfiguration(builder.Configuration);

            var app = builder.Build();

            await app.SeedDatabaseAsync();

            app.UseApplicationMiddlewares();
            app.Run();
        }
    }
}
