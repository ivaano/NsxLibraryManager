using Microsoft.EntityFrameworkCore;


namespace NsxLibraryManager.Extensions;

public static class DatabaseMigrationExtensions
{
    public static IApplicationBuilder EnsureDatabaseMigrated<TContext>(
        this IApplicationBuilder app,
        Action<TContext, IServiceProvider>? seeder = null) 
        where TContext : DbContext
    {
        using var scope = app.ApplicationServices.CreateScope();
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<TContext>();
        var logger = services.GetRequiredService<ILogger<TContext>>();
            
        try
        {
            var dbExists = context.Database.CanConnect();
                
            if (!dbExists)
            {
                // If database doesn't exist, create it
                context.Database.Migrate();
                logger.LogInformation("Database created and migrations applied.");
            }
            else
            {
                if (context.Database.GetPendingMigrations().Any())
                {
                    logger.LogInformation("Applying pending migrations...");
                    context.Database.Migrate();
                    logger.LogInformation("Migrations applied successfully.");
                }
            }
                
            // Run optional seeder
            if (seeder is not null)
            {
                seeder(context, services);
                logger.LogInformation("Seeding completed.");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while migrating or seeding the database.");
            throw;
        }

        return app;
    }
}