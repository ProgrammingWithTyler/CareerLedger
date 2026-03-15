using CareerLedger.Domain.Entities;
using CareerLedger.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace CareerLedger.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<CareerLedger.Domain.Entities.Application> Applications => Set<CareerLedger.Domain.Entities.Application>();
    public DbSet<ApplicationEvent> ApplicationEvents => Set<ApplicationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AccountConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationConfiguration());
        modelBuilder.ApplyConfiguration(new ApplicationEventConfiguration());
    }
}
