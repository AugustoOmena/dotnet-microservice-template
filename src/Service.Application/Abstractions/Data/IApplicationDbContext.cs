using Microsoft.EntityFrameworkCore;
using Service.Domain.Items;

namespace Service.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Item> Items { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}