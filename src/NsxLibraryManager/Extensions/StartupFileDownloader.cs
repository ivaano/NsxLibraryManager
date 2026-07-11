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

        await RefreshTitleDb(builder.Configuration);
        return builder;
    }

    // Downloads titledb if the remote version differs from the local one, independent of the
    // DownloadTitleDbOnStartup flag. Shared by the startup path and the --refresh-titledb CLI path.
    // Returns true if titledb was updated. Throws on download/decompression failure so a scheduler
    // invoking the CLI sees a non-zero exit code.
    public static async Task<bool> RefreshTitleDb(IConfiguration configuration)
    {
        var repoUrl = configuration.GetValue<string>($"{AppConstants.AppSettingsSectionName}:{AppConstants.AppSettingsSqlTitleDbRepository}");
        if (repoUrl is null)
        {
            Console.WriteLine($"Titledb repository URL is missing in config.json");
            return false;
        }
        var versionUrl = repoUrl.TrimEnd('/') + "/" + AppConstants.TitleDbVersionFile.TrimStart('/');
        var titledbUrl = repoUrl.TrimEnd('/') + "/" + AppConstants.TitleDbFile.TrimStart('/');

        var versionsFile = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigDirectory, AppConstants.TitleDbVersionFile);
        var compressedFilePath = Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory, AppConstants.TitleDbFile);
        var decompressedFilePath = Path.Combine(AppContext.BaseDirectory, AppConstants.DataDirectory, AppConstants.DefaultTitleDbName);
        var tempFilePath = decompressedFilePath + ".tmp";

        var localVersion = string.Empty;
        if (File.Exists(versionsFile))
        {
            localVersion = await File.ReadAllTextAsync(versionsFile);
        }

        var updated = false;
        try
        {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(versionUrl);
            var remoteVersion = response.TrimEnd('\r', '\n');

            if (remoteVersion.Length != 32)
            {
                Console.WriteLine($"Invalid titledb version number from {versionUrl}, skipping download");
                return false;
            }

            if (localVersion == remoteVersion)
            {
                Console.WriteLine($"Titledb already up to date (version {remoteVersion})");
                return false;
            }

            Console.WriteLine($"Downloading titledb version {remoteVersion}");
            var fileBytes = await httpClient.GetByteArrayAsync(titledbUrl);
            await File.WriteAllBytesAsync(compressedFilePath, fileBytes);

            // Decompress to a temp file, then atomically move it into place. A running app keeps
            // serving from its already-open file handle until it restarts, rather than reading a
            // half-written database mid-download.
            await using (var compressedFileStream = new FileStream(compressedFilePath, FileMode.Open, FileAccess.Read))
            await using (var decompressedFileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            await using (var gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress))
            {
                await gzipStream.CopyToAsync(decompressedFileStream);
                await gzipStream.FlushAsync();
            }

            File.Move(tempFilePath, decompressedFilePath, overwrite: true);
            await File.WriteAllTextAsync(versionsFile, remoteVersion);
            updated = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error downloading file: {ex.Message}");
            throw new InvalidOperationException("Required file could not be downloaded", ex);
        }
        finally
        {
            if (File.Exists(compressedFilePath))
            {
                File.Delete(compressedFilePath);
            }
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        return updated;
    }
}
