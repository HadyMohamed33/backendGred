using AlNady.Application.Interfaces;
using AlNady.Application.Interfaces.Repositories;
using AlNady.Infrastructure.Hubs;
using AlNady.Infrastructure.Persistence;
using AlNady.Infrastructure.Repositories;
using AlNady.Infrastructure.Services;
using Amazon.S3;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlNady.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // --- EF Core ---
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)
                       .EnableRetryOnFailure(3)
            )
        );

        // Register IApplicationDbContext — Application layer uses this abstraction
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        // --- Repositories & UoW ---
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // --- Core Services ---
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<INotificationService, NotificationService>();

        // --- File Storage ---
        var storageProvider = configuration["FileStorage:Provider"] ?? "Local";
        if (storageProvider.Equals("S3", StringComparison.OrdinalIgnoreCase))
        {
            services.AddDefaultAWSOptions(configuration.GetAWSOptions());
            services.AddAWSService<IAmazonS3>();
            services.AddScoped<IFileStorageService, S3StorageProvider>();
        }
        else
        {
            services.AddScoped<IFileStorageService, LocalStorageProvider>();
        }

        // --- Payment Services (both registered, resolved by ProviderName) ---
        services.AddHttpClient("Paymob", c =>
        {
            c.BaseAddress = new Uri("https://accept.paymob.com/api/");
            c.DefaultRequestHeaders.Add("Accept", "application/json");
        });
        services.AddScoped<IPaymentService, PaymobProvider>();
        services.AddScoped<IPaymentService, StripeProvider>();

        // --- Hangfire ---
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(
                configuration.GetConnectionString("HangfireConnection")
                    ?? configuration.GetConnectionString("DefaultConnection"),
                new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                    QueuePollInterval = TimeSpan.Zero,
                    UseRecommendedIsolationLevel = true,
                    DisableGlobalLocks = true
                }
            ));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = 5;
            options.Queues = new[] { "critical", "default", "low" };
        });

        // --- SignalR ---
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
            options.KeepAliveInterval = TimeSpan.FromSeconds(15);
        });

        // --- Redis Cache (optional) ---
        var redisConn = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConn))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = "AlNady:";
            });
        }
        else
        {
            services.AddMemoryCache();
        }

        return services;
    }
}
