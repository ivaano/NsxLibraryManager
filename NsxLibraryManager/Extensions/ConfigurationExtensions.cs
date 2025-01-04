using NsxLibraryManager.Data;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Providers;

namespace NsxLibraryManager.Extensions;

public static class ConfigurationExtensions
{
    public static void SetValue(this IConfiguration configuration, string key, string value)
    {
        var provider = (configuration as IConfigurationRoot)?.Providers
            .OfType<LiveDatabaseConfigurationProvider>()
            .FirstOrDefault();
            
        if (provider != null)
        {
            provider.SetValue(key, value);
            (configuration as IConfigurationRoot)?.Reload();
        }
    }

    public static IConfigurationBuilder AddLiveDatabase(
        this IConfigurationBuilder builder,
        IServiceScopeFactory scopeFactory)
    {
        return builder.Add(new LiveDatabaseConfigurationSource(scopeFactory));
    }
}
/*
public static class ConfigurationExtensions
{
    public static void SetValue(this IConfiguration configuration, string key, string value)
    {
        var sp = (configuration as IConfigurationRoot)?.Providers
            .OfType<DatabaseConfigurationProvider>()
            .FirstOrDefault()?.ScopeFactory;
            
        if (sp == null) return;
        
        using var scope = sp.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NsxLibraryDbContext>();
        
        var setting = dbContext.Setting.FirstOrDefault(s => s.Key == key) 
                      ?? new Setting { Key = key };
        setting.Value = value;
        
        dbContext.Setting.Update(setting);
        dbContext.SaveChanges();
        
        // Reload configuration
        (configuration as IConfigurationRoot)?.Reload();
    }
    
    public static IConfigurationBuilder AddLiveDatabase(
        this IConfigurationBuilder builder,
        IServiceScopeFactory scopeFactory)
    {
        return builder.Add(new DatabaseConfigurationSource(scopeFactory));
    }
}
*/