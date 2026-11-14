using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Service.Application.Abstractions.Data;
using Service.Domain.Items;
using Service.Infrastructure.Outbox;

namespace Service.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Item> Items => Set<Item>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}