using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared
{
    public static class DatabaseExtensions
    {
        public static async Task EnsureDatabaseAndMigrationsAsync<TDbContext>(
            this WebApplication app)
            where TDbContext : DbContext
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<TDbContext>>();
            var context = services.GetRequiredService<TDbContext>();

            try
            {
                logger.LogInformation("Checking database status for {DbContext}...",
                    typeof(TDbContext).Name);

                // بررسی اتصال
                var canConnect = await context.Database.CanConnectAsync();
                logger.LogInformation("Database connection: {Status}",
                    canConnect ? "Successful" : "Failed - Will attempt to create");

                if (!canConnect)
                {
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Database created and migrations applied successfully.");
                }
                else
                {
                    var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                    if (pendingMigrations.Any())
                    {
                        logger.LogInformation("Found {Count} pending migrations, applying...",
                            pendingMigrations.Count());
                        await context.Database.MigrateAsync();
                        logger.LogInformation("Database migrations completed successfully.");
                    }
                    else
                    {
                        logger.LogInformation("Database is already up-to-date with all migrations.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to migrate database {DbContext}",
                    typeof(TDbContext).Name);
                throw;
            }
        }
    }
}
