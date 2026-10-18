using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Persistence.Contexts;

/// <summary>Creates the context for EF migration commands without starting the API or Redis.</summary>
public sealed class SqlServerDatabaseContextFactory : IDesignTimeDbContextFactory<SqlServerDatabaseContext>
{
    public SqlServerDatabaseContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__SqlServerDatabaseContext");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Set ConnectionStrings__SqlServerDatabaseContext before running EF migration commands.");

        var options = new DbContextOptionsBuilder<SqlServerDatabaseContext>()
            .UseSqlServer(connectionString).Options;
        return new SqlServerDatabaseContext(options);
    }
}
