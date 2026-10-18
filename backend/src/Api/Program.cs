using Api.Middleware;
using Application;
using Asp.Versioning;
using Infrastructure;
using Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

namespace Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddMvc().AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<ApiExceptionHandler>();
        
        await builder.ConfigureInfrastructureLayer();
        builder.ConfigureApplicationLayer();
        
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var sqlServerDatabaseContext = scope.ServiceProvider.GetRequiredService<SqlServerDatabaseContext>();
        var initializer = scope.ServiceProvider.GetRequiredService<SqlServerDatabaseContextInitializer>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            await sqlServerDatabaseContext.Database.MigrateAsync();
            await initializer.InitializeAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or initializing the SQL Server database.");
            throw;
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseExceptionHandler();
        app.UseStatusCodePages();
        // The local Angular dev proxy forwards /api over the loopback HTTP endpoint.
        // Keep HTTPS redirection for deployed environments without redirecting proxied requests across origins.
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
