namespace API.Extensions
{
    public static class MiddlewareExtensions
    {
        public static WebApplication UseApplicationMiddlewares(this WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.MapOpenApi();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("FrontendCors");

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
