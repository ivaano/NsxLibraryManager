using System.IO.Compression;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Services;

public class TitledbService : ITitledbService
{
    //private readonly SettingsService _settingsService;
    //private readonly string _databasePath;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TitledbService> _logger;
    private readonly IConfiguration _configuration;


    public TitledbService(
        ILogger<TitledbService> logger,
        IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
    }

    private void DecompressFile(string compressedFilePath, string decompressedFilePath)
    {
        using var compressedFileStream = new FileStream(compressedFilePath, FileMode.Open, FileAccess.Read);
        using var decompressedFileStream = new FileStream(decompressedFilePath, FileMode.Create, FileAccess.Write);
        using var gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);
        gzipStream.CopyTo(decompressedFileStream);
    }

    public Task<string?> GetLatestTitledbVersionAsync()
    {
        throw new NotImplementedException();
    }

    public Task ReplaceDatabase(string compressedFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var databasePath = _configuration.GetSection("NsxLibraryManager:TitledbDBConnection").Value.CleanDatabasePath();

            if (!string.IsNullOrEmpty(databasePath))
            {
                
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    // Important: Do NOT retrieve the context here yet!
                    var destPath = compressedFilePath.Remove(compressedFilePath.IndexOf(".gz", StringComparison.Ordinal));
                    DecompressFile(compressedFilePath, destPath);
                    File.Copy(destPath, databasePath, true);
                    var fileInfo = new FileInfo(databasePath);

                    if (fileInfo.Length != new FileInfo(databasePath).Length)
                    {
                        throw new Exception("New titldb copy seems incomplete. Please retry.");
                    }
                
                    using (var newContext = scope.ServiceProvider.GetRequiredService<TitledbDbContext>())
                    {
                        // Verify the new database
                        var count = newContext.Titles.Count();
                        _logger.LogInformation("Number of titles in new db: {}", count);
                    }
                }
            }
            else
            {
                _logger.LogError("Unable to get titledb db connection from config.json.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error replacing database: {}", ex.Message);
            throw;
        }

        return Task.CompletedTask;
    }
}