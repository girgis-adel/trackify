using Common.Options;
using Domain.Entities.Identity;
using Infrastructure.Persistence.Contexts;
using Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
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
        builder.Services.AddScoped<IInterceptor, SoftRemovableInterceptor>();
        builder.Services.AddScoped<IInterceptor, AuditableInterceptor>();

        builder.Services.AddDbContext<SqlServerDatabaseContext>((sp, options) =>
        {
            options.UseSqlServer(builder.Configuration.GetConnectionString(nameof(SqlServerDatabaseContext)));

            options.AddInterceptors(sp.GetServices<IInterceptor>());
        });

        await builder.ConfigureCachingAsync();

        builder.ConfigureIdentity();

        builder.Services.AddOptions<SuperAdminUser>()
            .Bind(builder.Configuration.GetSection(nameof(SuperAdminUser)))
            .ValidateDataAnnotations()
            .ValidateOnStart();

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


        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwtOptions =>
            {
                jwtOptions.RequireHttpsMetadata = false;
                jwtOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SecretKey)),

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
