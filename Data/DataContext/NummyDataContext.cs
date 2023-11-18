using Microsoft.EntityFrameworkCore;
using Nummy.HttpLogger.Data.Entitites;

namespace Nummy.HttpLogger.Data.DataContext;

internal class NummyDataContext : DbContext
{
    public NummyDataContext(DbContextOptions<NummyDataContext> options) : base(options)
    {
    }

    public DbSet<NummyRequestLog> NummyRequestLogs { get; set; }
    public DbSet<NummyResponseLog> NummyResponseLogs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseLazyLoadingProxies();
        base.OnConfiguring(optionsBuilder);
    }

    // Define your DbSet properties here
    // Example:
    // public DbSet<User> Users { get; set; }
}