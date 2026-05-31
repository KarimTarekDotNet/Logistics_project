using Microsoft.AspNetCore.Antiforgery;

namespace API.Middlewares
{
    public class AntiforgeryMiddleware
    {
        private readonly RequestDelegate _next;

        public AntiforgeryMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAntiforgery antiforgery)
        {
            var method = context.Request.Method;

            var isUnsafeMethod =
                method is "POST" or "PUT" or "PATCH" or "DELETE";

            var hasAuthCookie =
                context.Request.Cookies.ContainsKey("AuthToken");

            if (hasAuthCookie && isUnsafeMethod)
            {
                await antiforgery.ValidateRequestAsync(context);
            }

            await _next(context);
        }
    }
}
