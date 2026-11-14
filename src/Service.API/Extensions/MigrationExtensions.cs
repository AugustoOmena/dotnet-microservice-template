using Microsoft.EntityFrameworkCore;
using Service.Infrastructure.Persistence;

namespace Service.API.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}