using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Services;

public sealed class DownloadService(
    IOptions<AppSettings> appSettings,
    IHttpClientFactory httpClientFactory,
    ILogger<DownloadService> logger)
    : IDownloadService
{
    private readonly DownloadSettings _downloadSettings = appSettings.Value.DownloadSettings;

    public async Task<string> GetRegionFile(string region, CancellationToken cancellationToken = default)
    {
        if (_downloadSettings is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var url = _downloadSettings.RegionUrl.Replace("{region}", region) ??
                  throw new InvalidOperationException("DownloadRegionUrl is needed in config");
        var destFilePath = Path.Combine(_downloadSettings.TitleDbPath, $"{region}.en.json");
        logger.LogInformation($"Downloading region {region} from {url} to {destFilePath}");
        await DownloadFileAsync(url, destFilePath, cancellationToken);
        return destFilePath;
    }

    public async Task<string> GetCnmtsFile(CancellationToken cancellationToken = default)
    {
        if (_downloadSettings is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var destFilePath = Path.Combine(_downloadSettings.TitleDbPath, $"cnmts.json");
        await DownloadFileAsync(_downloadSettings.CnmtsUrl, destFilePath, cancellationToken);
        return destFilePath;
    }

    public async Task<string> GetVersionsFile(CancellationToken cancellationToken = default)
    {
        if (_downloadSettings is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var destFilePath = Path.Combine(_downloadSettings.TitleDbPath, $"versions.json");
        await DownloadFileAsync(_downloadSettings.VersionsUrl, destFilePath, cancellationToken);
        return destFilePath;
    }

    public async Task DownloadFileAsync(string url, string destFilePath, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(_downloadSettings.TimeoutInSeconds);
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            if (response is { IsSuccessStatusCode: false, ReasonPhrase: not null }) throw new SimpleHttpResponseException(response.StatusCode, response.ReasonPhrase);

            var directory = Path.GetDirectoryName(destFilePath);
            
            if (directory is not null)
                if (Directory.Exists(directory) == false)
                    Directory.CreateDirectory(directory);
            
            await using var fileStream = File.Create(destFilePath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);
        }
        catch (SimpleHttpResponseException e)
        {
            logger.LogError(e, "{Url} returned {EStatusCode} with message {EMessage}", url, e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error saving file {DestFilePath} with message {EMessage}", destFilePath, e.Message);
        }
    }
}