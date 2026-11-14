using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Service.Application.Abstractions.Authentication;
using Service.Application.Abstractions.Data;
using Service.Infrastructure.Authentication;
using Service.Infrastructure.Persistence;
using Service.Infrastructure.Persistence.Interceptors;

namespace Service.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        AddPersistence(services, configuration);
        AddAuthentication(services, configuration);

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database")
            ?? throw new InvalidOperationException("Connection string 'Database' não configurada.");

        services.AddSingleton<AuditableEntityInterceptor>();
        services.AddSingleton<OutboxMessagesInterceptor>();

        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                    npgsql.EnableRetryOnFailure(maxRetryCount: 3);
                })
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(
                    sp.GetRequiredService<AuditableEntityInterceptor>(),
                    sp.GetRequiredService<OutboxMessagesInterceptor>());
        });

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<AppDbContext>());
    }

    private static void AddAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(JwtOptions.SectionName);
        var jwtOptions = jwtSection.Get<JwtOptions>()
            ?? throw new InvalidOperationException($"Seção '{JwtOptions.SectionName}' não configurada.");

        if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey) || Encoding.UTF8.GetByteCount(jwtOptions.SecretKey) < 32)
        {
            throw new InvalidOperationException(
                "A configuração 'Jwt:SecretKey' deve possuir no mínimo 32 bytes (256 bits) para assinatura HMAC-SHA256 segura.");
        }

        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) || string.IsNullOrWhiteSpace(jwtOptions.Audience))
        {
            throw new InvalidOperationException(
                "As configurações 'Jwt:Issuer' e 'Jwt:Audience' são obrigatórias.");
        }

        services.Configure<JwtOptions>(jwtSection);

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
    }
}