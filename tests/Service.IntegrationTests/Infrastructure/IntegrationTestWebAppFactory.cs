using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Service.Infrastructure.Persistence;
using Service.Infrastructure.Persistence.Interceptors;
using Testcontainers.PostgreSql;

namespace Service.IntegrationTests.Infrastructure;

public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:17-alpine")
        .WithDatabase("microservice_test_db")
        .WithUsername("test_user")
        .WithPassword("TestPassword123!")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options
                    .UseNpgsql(_dbContainer.GetConnectionString())
                    .UseSnakeCaseNamingConvention()
                    .AddInterceptors(
                        sp.GetRequiredService<AuditableEntityInterceptor>(),
                        sp.GetRequiredService<OutboxMessagesInterceptor>());
            });
        });
    }

    public Task InitializeAsync() => _dbContainer.StartAsync();

    public new Task DisposeAsync() => _dbContainer.DisposeAsync().AsTask();
}