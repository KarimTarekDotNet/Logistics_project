using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace API.Filters.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IntegrationKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string IntegrationKeyHeader = "X-Integration-Key";
        private const string IdempotencyKeyHeader = "Idempotency-Key";
        private const string CorrelationIdHeader = "X-Correlation-Id";
        private const string IntegrationSourceHeader = "X-Integration-Source";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            var headers = context.HttpContext.Request.Headers;

            if (!headers.TryGetValue(IntegrationKeyHeader, out var providedKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Missing integration key" });
                return;
            }

            if (!headers.TryGetValue(IdempotencyKeyHeader, out var idempotencyKey))
            {
                context.Result = new BadRequestObjectResult(new { message = "Missing idempotency key" });
                return;
            }

            if (!headers.TryGetValue(IntegrationSourceHeader, out var integrationSource))
            {
                context.Result = new BadRequestObjectResult(new { message = "Missing integration source" });
                return;
            }

            headers.TryGetValue(CorrelationIdHeader, out var correlationId);

            var expectedKey = configuration["Integrations:N8n:ApiKey"];

            if (string.IsNullOrWhiteSpace(expectedKey) || providedKey != expectedKey)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid integration key" });
                return;
            }

            if (string.IsNullOrWhiteSpace(correlationId.ToString()))
            {
                correlationId = Guid.NewGuid().ToString();
            }

            context.HttpContext.Items["CorrelationId"] = correlationId.ToString();
            context.HttpContext.Items["IntegrationSource"] = integrationSource.ToString();
            context.HttpContext.Items["IdempotencyKey"] = idempotencyKey.ToString();
            context.HttpContext.Items["IntegrationKey"] = providedKey.ToString();

            await next();
        }
    }
}