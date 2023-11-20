﻿using System.Net;
using Castle.Core.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using ILogger = Castle.Core.Logging.ILogger;

namespace Tests.Services;

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
                Regions = new[] { "US" },
                RegionUrl = "https://raw.githubusercontent.com/blawar/titledb/master/{region}.en.json",
                CnmtsUrl = "https://raw.githubusercontent.com/blawar/titledb/master/cnmts.json",
                VersionsUrl = "https://raw.githubusercontent.com/blawar/titledb/master/versions.json",
        };
        var appSettings = new AppSettings
        {
                DownloadSettings = downloadSettings,
                TitleDatabase = "titledb.db",
                LibraryPath = "somepath",
        };
        var options = Options.Create(appSettings);
        
        _downloadService = new DownloadService(options, _httpClientFactory, _logger);
    }
    
    [Fact]
    public async Task Should_Download_Region_File()
    {
        //Arrange
        var testRegion = "US";
        var messageHandler = new MockHttpMessageHandler("TEST VALUE", HttpStatusCode.OK);
        var httpClient = new HttpClient(messageHandler);
        _httpClientFactory.CreateClient().Returns(httpClient);
        var cancellationToken = new CancellationToken();
        
        //Act
        var destFilePath = await _downloadService.GetRegionFile(testRegion, cancellationToken);
        
        //Assert
        Assert.Equal("TEST VALUE", File.ReadAllText(destFilePath));
 
    }

}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly string _response;
    private readonly HttpStatusCode _statusCode;

    public string Input { get; private set; }
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