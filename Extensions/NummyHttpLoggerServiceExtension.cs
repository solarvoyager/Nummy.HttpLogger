using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nummy.HttpLogger.Middleware;
using Nummy.HttpLogger.Models;

namespace Nummy.HttpLogger.Extensions
{
    public static class NummyHttpLoggerServiceExtension
    {
        public static void AddNummyHttpLogger(this IServiceCollection services, Action<NummyHttpLoggerOptions> options)
        {
            services.Configure(options);
        }

        public static void UsNummyHttpLogger(IApplicationBuilder app)
        {
            app.UseMiddleware<NummyHttpLoggerMiddleware>();
        }

    }
}
