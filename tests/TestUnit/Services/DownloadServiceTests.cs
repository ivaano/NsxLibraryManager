using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Shared.Settings;

namespace TestUnit.Services;

public class DownloadServiceTests
{
    private readonly IHttpClientFactory _httpClientFactory = Substitute.For<IHttpClientFactory>();
    private readonly IDownloadService _downloadService;
    private readonly ILogger<DownloadService> _logger = Substitute.For<ILogger<DownloadService>>();

    public DownloadServiceTests()
    {
        var downloadSettings = new DownloadSettings
        {
            TimeoutInSeconds = 10,
            TitleDbPath = "/tmp",
            TitleDbUrl = "changos",
            VersionUrl = "version",

        };
        var appSettings = new UserSettings
        {
            DownloadSettings = downloadSettings,
            TitleDatabase = "titledb.db",
            LibraryPath = "somepath",
        };
        var options = Options.Create(appSettings);
        
        _downloadService = new DownloadService(_httpClientFactory, _logger);
    }
   
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _response;
    private readonly HttpStatusCode _statusCode;

    public string? Input { get; private set; }
    public int NumberOfCalls { get; private set; }

    public MockHttpMessageHandler(string response, HttpStatusCode statusCode)
    {
        _response = response;
        _statusCode = statusCode;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
    {
        NumberOfCalls++;
        if (request.Content != null) // Could be a GET-request without a body
        {
            Input = await request.Content.ReadAsStringAsync();
        }
        return new HttpResponseMessage
        {
                StatusCode = _statusCode,
                Content = new StringContent(_response)
        };
    }
}