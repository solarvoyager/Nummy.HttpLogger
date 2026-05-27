using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Nummy.HttpLogger.Data.Services;
using Nummy.HttpLogger.Middleware;
using Nummy.HttpLogger.Utils;

namespace Nummy.HttpLogger.Extensions;

public static class NummyHttpLoggerServiceExtension
{
    public static IServiceCollection AddNummyHttpLogger(this IServiceCollection services,
        Action<NummyHttpLoggerOptions> options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        var httpLoggerOptions = new NummyHttpLoggerOptions();
        options.Invoke(httpLoggerOptions);

        NummyValidators.ValidateNummyHttpLoggerOptions(httpLoggerOptions);

        services.Configure(options);

        services.AddSingleton<INummyHttpLoggerService, NummyHttpLoggerService>();

        services.AddHttpClient(NummyConstants.ClientName, config =>
        {
            config.BaseAddress = new Uri(httpLoggerOptions.NummyServiceUrl!);
            config.Timeout = httpLoggerOptions.HttpClientTimeout;
            config.DefaultRequestHeaders.Clear();
        });

        return services;
    }

    public static void UseNummyHttpLogger(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        app.UseMiddleware<NummyHttpLoggerMiddleware>();
    }
}