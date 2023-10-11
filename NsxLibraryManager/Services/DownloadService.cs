using Microsoft.Extensions.Options;
using NsxLibraryManager.Settings;
using Nsxrenamer.Exceptions;
using Nsxrenamer.Settings;

namespace NsxLibraryManager.Services;

public sealed class DownloadService : IDownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly DownloadSettings _downloadSettings;
    private readonly ILogger<DownloadService> _logger;

    public DownloadService(
            IOptions<AppSettings> configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<DownloadService> logger) =>
            (_downloadSettings, _httpClientFactory, _logger) = (configuration.Value.DownloadSettings, httpClientFactory, logger);


    public async Task<string> GetRegionFile(string region, CancellationToken cancellationToken = default)
    {
        if (_downloadSettings is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var url = _downloadSettings.RegionUrl.Replace("{region}", region) ??
                  throw new InvalidOperationException("DownloadRegionUrl is needed in config");
        var destFilePath = Path.Combine(_downloadSettings.TitleDbPath, $"{region}.en.json");
        _logger.LogInformation($"Downloading region {region} from {url} to {destFilePath}");
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
        using var client = _httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(_downloadSettings.TimeoutInSeconds);
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode == false)
                throw new SimpleHttpResponseException(response.StatusCode, response.ReasonPhrase);
            
            var directory = Path.GetDirectoryName(destFilePath);
            
            if (directory is not null)
                if (Directory.Exists(directory) == false)
                    Directory.CreateDirectory(directory);
            
            await using var fileStream = File.Create(destFilePath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);
        }
        catch (SimpleHttpResponseException e)
        {
            _logger.LogError(e, "{Url} returned {EStatusCode} with message {EMessage}", url, e.StatusCode, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error saving file {DestFilePath} with message {EMessage}", destFilePath, e.Message);
        }
    }
}