using API.Middlewares;

namespace API.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomGlobalHandleException(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalHandleExceptionMiddleware>();
        }
    }
}
