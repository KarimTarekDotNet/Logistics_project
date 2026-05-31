using API.Middlewares;

namespace API.Extensions
{
    public static class AntiforgeryMiddlewareExtensions
    {
        public static IApplicationBuilder UseCustomAntiforgery(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AntiforgeryMiddleware>();
        }
    }
}