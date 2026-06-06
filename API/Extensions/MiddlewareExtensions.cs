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

            app.UseHttpsRedirection();

            var corsPolicy = configuration.GetValue<string>("CORS:CorsPolicy")!;
            app.UseCors(corsPolicy);

            app.UseStaticFiles();
            app.UseAuthentication();

            app.UseCustomGlobalHandleException();
            app.UseCustomAntiforgery();

            app.UseRateLimiter();

            app.UseAuthorization();

            app.MapControllers();

            return app;
        }
    }
}
