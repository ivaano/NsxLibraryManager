using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Data;
using NsxLibraryManager.Providers;

namespace NsxLibraryManager.Utils;

public class StartupConfiguration
{
    public static WebApplicationBuilder ConfigureServices(string[] args, IConfigurationRoot initialConfig)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Add DB context with connection string from initial config
        builder.Services.AddDbContext<NsxLibraryDbContext>(options =>
            options.UseSqlite(initialConfig.GetSection("NsxLibraryManager:NsxLibraryDbConnection").Value));

        //initialConfig.GetSection("NsxLibraryManager:NsxLibraryDbConnection")
        
        // Rest of database configuration
        builder.Services.AddSingleton<IConfigurationSource>(sp => 
            new DatabaseConfigurationSource(initialConfig, sp.GetRequiredService<IServiceScopeFactory>()));

        var dbConfigSource = new DatabaseConfigurationSource(initialConfig, builder.Services.BuildServiceProvider().GetRequiredService<IServiceScopeFactory>());
        var dbConfig = new ConfigurationBuilder()
            .Add(dbConfigSource)
            .Build();

        builder.Configuration.AddConfiguration(dbConfig);
        return builder;
    }
}