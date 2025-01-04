using NsxLibraryManager.Data;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Providers;

public class DatabaseConfigurationProvider : ConfigurationProvider
{
    private readonly IConfiguration _configuration;
    internal readonly IServiceScopeFactory ScopeFactory;
    
    public DatabaseConfigurationProvider(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        ScopeFactory = scopeFactory;
    }

    public override void Load()
    {
        using var scope = ScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NsxLibraryDbContext>();

        // Ensure database exists and create if needed
        dbContext.Database.EnsureCreated();

        // Load settings from database
        var settings = dbContext.Setting.ToDictionary(s => s.Key, s => s.Value);
        
        // If no settings exist, create defaults
        if (!settings.Any())
        {
            var defaultSettings = CreateDefaultSettings();
            dbContext.Setting.AddRange(defaultSettings.Select(s => new Setting { Key = s.Key, Value = s.Value }));
            dbContext.SaveChanges();
            settings = defaultSettings;
        }

        Data = settings;
    }
    
    private Dictionary<string, string> CreateDefaultSettings()
    {
        // Convert your default JSON settings to key-value pairs
        return new Dictionary<string, string>
        {
            { "NsxLibraryManager:DownloadSettings:TitleDbPath", Path.Combine(AppContext.BaseDirectory, "titledb") },
            { "NsxLibraryManager:DownloadSettings:TimeoutInSeconds", "100" },
            { "NsxLibraryManager:DownloadSettings:RegionUrl", "https://raw.githubusercontent.com/blawar/titledb/master/{region}.en.json" },
            { "NsxLibraryManager:DownloadSettings:CnmtsUrl", "https://raw.githubusercontent.com/blawar/titledb/master/cnmts.json" },
            { "NsxLibraryManager:DownloadSettings:VersionsUrl", "https://raw.githubusercontent.com/blawar/titledb/master/versions.json" },
            { "NsxLibraryManager:DownloadSettings:Regions", """["US"]""" },
            { "NsxLibraryManager:LibraryPath", Path.Combine(AppContext.BaseDirectory, "library") },
            { "NsxLibraryManager:TitleDatabase", Path.Combine(AppContext.BaseDirectory, "NsxlibraryManager.db") },
        };
    }
}