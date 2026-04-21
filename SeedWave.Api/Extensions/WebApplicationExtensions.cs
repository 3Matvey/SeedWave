using SeedWave.Api.Middleware;

namespace SeedWave.Api.Extensions
{
    public static class WebApplicationExtensions
    {
        public static WebApplication UseSeedWave(this WebApplication app)
        {
            app.UseMiddleware<ExceptionHandlingMiddleware>();

            return app;
        }
    }
}
