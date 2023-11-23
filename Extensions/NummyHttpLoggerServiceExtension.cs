using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nummy.HttpLogger.Data.DataContext;
using Nummy.HttpLogger.Data.Services;
using Nummy.HttpLogger.Middleware;
using Nummy.HttpLogger.Models;
using Nummy.HttpLogger.Utils;

namespace Nummy.HttpLogger.Extensions;

public static class NummyHttpLoggerServiceExtension
{
    public static IServiceCollection AddNummyHttpLogger(this IServiceCollection services,
        Action<NummyHttpLoggerOptions> options)
    {
        var httpLoggerOptions = new NummyHttpLoggerOptions();
        options.Invoke(httpLoggerOptions);

        NummyModelValidator.ValidateNummyHttpLoggerOptions(httpLoggerOptions);

        services.Configure(options);

        services.AddDbContext<NummyHttpLoggerDataContext>(dbOptions =>
            dbOptions.UseNpgsql(httpLoggerOptions.DatabaseConnectionString));

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        services.AddScoped<INummyHttpLoggerService, NummyHttpLoggerService>();

        // Automatically apply migrations during startup
        using var serviceScope = services.BuildServiceProvider().CreateScope();
        {
            var dbContext = serviceScope.ServiceProvider.GetRequiredService<NummyHttpLoggerDataContext>();

            // Ensure the database exists, and create it if not
            //dbContext.Database.EnsureCreated();

            if (dbContext.Database.GetPendingMigrations().Any())
            {
                // Apply pending migrations
                dbContext.Database.Migrate();
            }
        }

        return services;
    }

    public static void UseNummyHttpLogger(this IApplicationBuilder app)
    {
        app.UseMiddleware<NummyHttpLoggerMiddleware>();
    }
}