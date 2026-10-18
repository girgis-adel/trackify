using Common.Abstractions;
using Common.Options;
using Domain.Entities.Artists;
using Domain.Entities.DSPs;
using Domain.Entities.Identity;
using Domain.Entities.TrackDistributions;
using Domain.Entities.Tracks;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Interceptors;
using Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using System.Text;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed.Redis;
using Role = Domain.Entities.Identity.Role;

namespace Infrastructure;

public static class DependencyInjection
{
    public static async Task<IHostApplicationBuilder> ConfigureInfrastructureLayer(this IHostApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        builder.Services.AddScoped<IArtistRepository, ArtistRepository>();
        builder.Services.AddScoped<ITrackRepository, TrackRepository>();
        builder.Services.AddScoped<IDSPRepository, DSPRepository>();
        builder.Services.AddScoped<ITrackDistributionRepository, TrackDistributionRepository>();
        builder.Services.AddOptions<DemoOptions>()
            .Bind(builder.Configuration.GetSection(nameof(DemoOptions)));

        var demoEnabled = builder.Configuration.GetValue<bool>($"{nameof(DemoOptions)}:Enabled");
        if (demoEnabled && !builder.Environment.IsDevelopment())
            throw new InvalidOperationException("Demo mode is only supported in the Development environment.");
        var connectionString = builder.Configuration.GetConnectionString(nameof(SqlServerDatabaseContext));
        if (demoEnabled)
        {
            var demoConnectionString = builder.Configuration.GetConnectionString("DemoSqlServerDatabaseContext")
                ?? throw new InvalidOperationException("Demo mode requires a dedicated DemoSqlServerDatabaseContext connection string.");
            var demoConnection = new SqlConnectionStringBuilder(demoConnectionString);
            if (string.IsNullOrWhiteSpace(demoConnection.InitialCatalog))
                throw new InvalidOperationException("Demo mode requires an explicitly named isolated database.");

            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                var normalConnection = new SqlConnectionStringBuilder(connectionString);
                if (string.Equals(demoConnection.DataSource, normalConnection.DataSource, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(demoConnection.InitialCatalog, normalConnection.InitialCatalog, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidOperationException("The demo database must be separate from the normal application database.");
            }

            connectionString = demoConnectionString;
        }

        builder.Services.AddScoped<IInterceptor, SoftRemovableInterceptor>();
        builder.Services.AddScoped<IInterceptor, AuditableInterceptor>();

        builder.Services.AddDbContext<SqlServerDatabaseContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString);

            options.AddInterceptors(sp.GetServices<IInterceptor>());
        });

        await builder.ConfigureCachingAsync();

        builder.ConfigureIdentity();

        builder.Services.AddOptions<SuperAdminUser>()
            .Bind(builder.Configuration.GetSection(nameof(SuperAdminUser)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddTransient<SqlServerDatabaseContextInitializer>();

        return builder;
    }

    private static async Task<IHostApplicationBuilder> ConfigureCachingAsync(this IHostApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis")
            ?? throw new InvalidOperationException("Redis connection string is not configured.");

        var configurationOptions = ConfigurationOptions.Parse(connectionString!);
        configurationOptions.AbortOnConnectFail = false;
        configurationOptions.ConnectRetry = 5;
        configurationOptions.ConnectTimeout = 2500;
        configurationOptions.SyncTimeout = 250;
        configurationOptions.ResolveDns = true;

        IConnectionMultiplexer muxer = await ConnectionMultiplexer.ConnectAsync(configurationOptions);

        builder.Services.AddSingleton(muxer);

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConnectionMultiplexerFactory = () => Task.FromResult(muxer);
        });

        builder.Services.AddFusionCache()
            .WithOptions(options =>
            {
                options.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(10);
            })
            .WithDefaultEntryOptions(new FusionCacheEntryOptions
            {
                Duration = TimeSpan.FromMinutes(2),
                DistributedCacheDuration = TimeSpan.FromMinutes(15),

                IsFailSafeEnabled = true,
                FailSafeMaxDuration = TimeSpan.FromHours(2),
                FailSafeThrottleDuration = TimeSpan.FromSeconds(30),

                EagerRefreshThreshold = 0.9f,

                FactorySoftTimeout = TimeSpan.FromMilliseconds(100),
                FactoryHardTimeout = TimeSpan.FromMilliseconds(1500),

                DistributedCacheSoftTimeout = TimeSpan.FromSeconds(1),
                DistributedCacheHardTimeout = TimeSpan.FromSeconds(2),
                AllowBackgroundDistributedCacheOperations = true,

                JitterMaxDuration = TimeSpan.FromSeconds(2),
            })
            .WithSystemTextJsonSerializer()
            .WithRegisteredDistributedCache()
            .WithStackExchangeRedisBackplane(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(muxer);
            })
            .WithRedisDistributedLocker(options =>
            {
                options.ConnectionMultiplexerFactory = () => Task.FromResult(muxer);
            });

        return builder;
    }

    private static IHostApplicationBuilder ConfigureIdentity(this IHostApplicationBuilder builder)
    {
        var options = builder.Configuration.GetSection(nameof(AppJwtBearerOptions)).Get<AppJwtBearerOptions>()
            ?? throw new InvalidOperationException("AppJwtBearerOptions is not configured");
        if (string.IsNullOrWhiteSpace(options.SecretKey) || Encoding.UTF8.GetByteCount(options.SecretKey) < 32 ||
            string.IsNullOrWhiteSpace(options.Issuer) || string.IsNullOrWhiteSpace(options.Audience) ||
            !double.IsFinite(options.ExpirationInMinutes) || options.ExpirationInMinutes is < 1 or > 1440)
            throw new InvalidOperationException("A valid JWT key (at least 32 bytes), issuer, audience, and expiration are required.");


        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),

                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidateIssuer = true,
                    ValidIssuer = options.Issuer,

                    ValidateAudience = true,
                    ValidAudiences = [options.Audience],

                    ClockSkew = TimeSpan.Zero
                };
            })
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
            {
                o.Cookie.Name = "x-rmembr";
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
                };
            })
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
            {
                o.Cookie.Name = "x-uid";
                o.Events = new CookieAuthenticationEvents
                {
                    OnRedirectToReturnUrl = _ => Task.CompletedTask
                };
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
            });

        builder.Services.AddOptions<AppJwtBearerOptions>()
            .Bind(builder.Configuration.GetSection(nameof(AppJwtBearerOptions)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddIdentityCore<User>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredUniqueChars = 4;
            options.Password.RequiredLength = 8;
            options.Lockout.AllowedForNewUsers = false;
        })
            .AddRoles<Role>()
            .AddSignInManager()
            .AddEntityFrameworkStores<SqlServerDatabaseContext>()
            .AddDefaultTokenProviders();

        builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromMinutes(7);
        });

        return builder;
    }
}
