using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Shared.Settings;

namespace NsxLibraryManager.Core.Services;

public sealed partial class DownloadService(
    IHttpClientFactory httpClientFactory,
    ILogger<DownloadService> logger)
    : IDownloadService
{

    [GeneratedRegex(@"\t|\n|\r")]
    private static partial Regex NoNewLines();
    
    public async Task<string> GetVersionsFile(string versionUrl, CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient();
        var response = await client.GetAsync(versionUrl, cancellationToken);
        if (!response.IsSuccessStatusCode) return string.Empty;
        
        var version = NoNewLines().Replace(await response.Content.ReadAsStringAsync(cancellationToken), ""); 
        return version;
    }

    public async Task<string> GetLatestTitleDb(DownloadSettings downloadSettings, CancellationToken cancellationToken)
    {
        //if (downloadSettings is { TitleDbPath: null,  TitleDbUrl: null})
        //    throw new InvalidOperationException("TitleDbPath and TitleDbUrl settings are not set.");
        
        var destFilePath = Path.Combine(downloadSettings.TitleDbPath, $"{AppConstants.DefaultTitleDbName}.gz");
        await DownloadFileAsync(downloadSettings.TitleDbUrl, destFilePath, downloadSettings.TimeoutInSeconds, cancellationToken);
        
        return destFilePath;
    }

    public async Task DownloadFileAsync(string url, string destFilePath, int timeOut, CancellationToken cancellationToken)
    {
        using var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(timeOut);
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