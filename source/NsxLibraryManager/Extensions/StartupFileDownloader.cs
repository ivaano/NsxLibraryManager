using System.IO.Compression;
using System.Text.RegularExpressions;
using LibHac.Util;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Extensions;

public static class StartupFileDownloader
{
    public static async Task<WebApplicationBuilder> TitleDbDownloader(this WebApplicationBuilder builder)
    {
        var downloadOnStartup = builder.Configuration.GetValue<bool>($"{AppConstants.AppSettingsSectionName}:{AppConstants.AppSettingsDownloadTitleDbOnStartup}");
        if (!downloadOnStartup)
        {
            Console.WriteLine($"Not downloading latest version of SqlTitleDb on Startup");
            return builder;
        }        
        var repoUrl = builder.Configuration.GetValue<string>($"{AppConstants.AppSettingsSectionName}:{AppConstants.AppSettingsSqlTitleDbRepository}");
        if (repoUrl is null)
        {
            Console.WriteLine($"Titledb repository URL is missing in config.json");
            return builder;
        }
        var versionUrl= repoUrl.TrimEnd('/') + "/" + AppConstants.TitleDbVersionFile.TrimStart('/'); 
        var titledbUrl= repoUrl.TrimEnd('/') + "/" + AppConstants.TitleDbFile.TrimStart('/'); 

        var versionsFile = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigDirectory, AppConstants.TitleDbVersionFile);
        var localVersion = string.Empty;
        var remoteVersion = string.Empty;
        var compressedFilePath = Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory,
            AppConstants.TitleDbFile);
        var decompressedFilePath = Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory,
            AppConstants.DefaultTitleDbName);
        if (File.Exists(versionsFile))
        {
            localVersion = await File.ReadAllTextAsync(versionsFile);
        }
        
        try
        {
            using var httpClient = new HttpClient();
            var request = httpClient.GetAsync(versionUrl);
            var response = await request.Result.Content.ReadAsStringAsync();
            remoteVersion = response.TrimEnd( '\r', '\n' );

            if (localVersion != remoteVersion)
            {
                Console.WriteLine($"Downloading titledb version {remoteVersion}");
                var fileBytes = await httpClient.GetByteArrayAsync(titledbUrl);
                await File.WriteAllBytesAsync(compressedFilePath, fileBytes);
                await using var compressedFileStream = new FileStream(compressedFilePath, FileMode.Open, FileAccess.Read);
                await using var decompressedFileStream = new FileStream(decompressedFilePath, FileMode.Create, FileAccess.Write);
                await using var gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);
                await gzipStream.CopyToAsync(decompressedFileStream);
                await gzipStream.FlushAsync();
                await File.WriteAllTextAsync(versionsFile, remoteVersion);
            }
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            throw new InvalidOperationException("Required file could not be downloaded during startup", ex);
        }

        if (File.Exists(compressedFilePath))
        {
            File.Delete(compressedFilePath);
        }



        return builder;
    }
}
