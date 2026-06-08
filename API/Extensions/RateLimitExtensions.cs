using Microsoft.AspNetCore.RateLimiting;

namespace API.Extensions
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection AddRateLimitConfiguration(this IServiceCollection services)
        {

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.AddFixedWindowLimiter("AuthPolicy", opt =>
                {
                    opt.PermitLimit = 10;
                    opt.Window = TimeSpan.FromMinutes(2);
                    opt.QueueLimit = 0;
                });

                options.AddFixedWindowLimiter("OtpPolicy", opt =>
                {
                    opt.PermitLimit = 5;
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

            return services;
        }
    }
}
