using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nummy.ExceptionHandler.Middlewares;
using Nummy.ExceptionHandler.Models;
using Nummy.HttpLogger.Services;

namespace Nummy.HttpLogger.Extensions
{
    public static class NummyHttpLoggerServiceExtension
    {
        public static void AddNummyLogger(this IServiceCollection services, Action<NummyLoggerOptions>? options = null)
        {
            services.AddScoped<INummyHttpLogger, NummyHttpLogger>();

            if (options is not null)
                services.Configure(options);
        }

        public static void UsNummyLogger(IApplicationBuilder app)
        {
            app.UseMiddleware<NummyExceptionMiddleware>();
        }

    }
}
