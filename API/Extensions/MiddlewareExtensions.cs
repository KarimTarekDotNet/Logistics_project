using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static WebApplication UseApplicationMiddlewares(this WebApplication app, IConfiguration configuration)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.MapOpenApi();
                app.UseSwaggerUI();
            }
            app.UseCustomGlobalHandleException();

            app.UseHttpsRedirection();

            var corsPolicy = configuration.GetValue<string>("CORS:CorsPolicy")!;
            app.UseCors(corsPolicy);

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseCustomAntiforgery();

            app.UseRateLimiter();

            app.UseAuthorization();

            //// expose /metrics
            //app.UseHttpMetrics();

            //app.MapMetrics();

            // 3. Add clean HTTP request logging middleware
            app.UseSerilogRequestLogging();

            app.MapControllers();

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            return app;
        }
    }
}