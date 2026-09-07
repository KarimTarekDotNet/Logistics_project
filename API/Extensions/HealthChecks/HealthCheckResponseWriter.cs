using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Extensions.HealthChecks
{
    public static class HealthCheckResponseWriter
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        public static Task WriteResponseAsync(HttpContext context, HealthReport report)
        {
            context.Response.ContentType = "application/json";
            var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
            
            var response = new
            {
                Status = report.Status.ToString(),
                TotalDurationInMilliseconds = report.TotalDuration.TotalMilliseconds,
                Checks = report.Entries.Select(entry => new
                {
                    Name = entry.Key,
                    Status = entry.Value.Status.ToString(),
                    DurationInMilliseconds = entry.Value.Duration.TotalMilliseconds,
                    Exception = env.IsDevelopment() ? entry.Value.Exception?.Message : null,
                    entry.Value.Description
                })
            };
            return context.Response.WriteAsJsonAsync(response, JsonOptions);
        }
    }
}