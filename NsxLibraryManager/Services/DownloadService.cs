using Microsoft.Extensions.Options;
using NsxLibraryManager.Settings;
using Nsxrenamer.Exceptions;

namespace NsxLibraryManager.Services;

public sealed class DownloadService : IDownloadService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly AppSettings _configuration;
    private readonly ILogger<DownloadService> _logger;

    public DownloadService(
            IOptions<AppSettings> configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<DownloadService> logger) =>
            (_configuration, _httpClientFactory, _logger) = (configuration.Value, httpClientFactory, logger);


    public async Task<string> GetRegionFile(string region, CancellationToken cancellationToken = default)
    {
        var downloadConfig = _configuration.DownloadSettings;
        if (downloadConfig is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var url = downloadConfig.RegionUrl.Replace("{region}", region) ??
                  throw new InvalidOperationException("DownloadRegionUrl is needed in config");
        var destFilePath = Path.Combine(downloadConfig.TitleDbPath, $"{region}.en.json");
        _logger.LogInformation($"Downloading region {region} from {url} to {destFilePath}");
        await DownloadFileAsync(url, destFilePath, cancellationToken);
        return destFilePath;
    }

    public async Task<string> GetCnmtsFile(CancellationToken cancellationToken = default)
    {
        var downloadConfig = _configuration.DownloadSettings;
        if (downloadConfig is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var destFilePath = Path.Combine(downloadConfig.TitleDbPath, $"cnmts.json");
        await DownloadFileAsync(downloadConfig.CnmtsUrl, destFilePath, cancellationToken);
        return destFilePath;
    }

    public async Task<string> GetVersionsFile(CancellationToken cancellationToken = default)
    {
        var downloadConfig = _configuration.DownloadSettings;
        if (downloadConfig is { TitleDbPath: null })
            throw new InvalidOperationException("TitleDbPath is needed in config");
        var destFilePath = Path.Combine(downloadConfig.TitleDbPath, $"versions.json");
        await DownloadFileAsync(downloadConfig.VersionsUrl, destFilePath, cancellationToken);
        return destFilePath;
    }

    public async Task DownloadFileAsync(string url, string destFilePath, CancellationToken cancellationToken)
    {
        using var client = _httpClientFactory.CreateClient();
        try
        {
            var response = await client.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode == false)
                throw new SimpleHttpResponseException(response.StatusCode, response.ReasonPhrase);
            await using var fileStream = File.Create(destFilePath);
            await response.Content.CopyToAsync(fileStream, cancellationToken);
        }
        catch (SimpleHttpResponseException e)
        {
            //await _consoleService.Print($"{url} returned {e.StatusCode} with message {e.Message}");
            _logger.LogError(e, $"{url} returned {e.StatusCode} with message {e.Message}");
        }
        catch (Exception e)
        {
            //await _consoleService.Print($"Error saving file {destFilePath} with message {e.Message}");
            _logger.LogError(e, $"Error saving file {destFilePath} with message {e.Message}");
        }
    }
}