using Microsoft.EntityFrameworkCore;
using Nummy.HttpLogger.Data.Entitites;

namespace Nummy.HttpLogger.Data.DataContext;

internal class NummyHttpLoggerDataContext : DbContext
{
    public NummyHttpLoggerDataContext(DbContextOptions<NummyHttpLoggerDataContext> options) : base(options)
    {
    }

    public DbSet<NummyRequestLog> NummyRequestLogs { get; set; }
    public DbSet<NummyResponseLog> NummyResponseLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        base.OnConfiguring(optionsBuilder);
    }
}