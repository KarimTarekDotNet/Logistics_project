using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace API.Filters.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IntegrationKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string HeaderName = "X-Integration-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();

            if(!context.HttpContext.Request.Headers.TryGetValue(HeaderName, out var providedKey))
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Missing integration key" });
                return;
            }
            var expectedKey = configuration["Integrations:N8n:ApiKey"];

            if (string.IsNullOrWhiteSpace(expectedKey) || providedKey != expectedKey)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Invalid integration key" });
                return;
            }

            await next();
        }
    }
}