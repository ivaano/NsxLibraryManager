using AutoFixture;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace Tests.Services;

public class TitleLibraryServiceTest
{
    private readonly IDataService _dataService = Substitute.For<IDataService>();
    private readonly IFileInfoService _fileInfoService = Substitute.For<IFileInfoService>();
    private readonly ITitleDbService _titleDbService = Substitute.For<ITitleDbService>();
    private readonly ILogger<TitleLibraryService> _logger = Substitute.For<ILogger<TitleLibraryService>>();
    private readonly ITitleLibraryService _titleLibraryService;
    
    public TitleLibraryServiceTest()
    {
        var appSettings = new AppSettings
        {
                DownloadSettings = new DownloadSettings
                {
                        TimeoutInSeconds = 10,
                        TitleDbPath = "/tmp",
                        Regions = new[] { "US" },
                        RegionUrl = "https://raw.githubusercontent.com/blawar/titledb/master/{region}.en.json",
                        CnmtsUrl = "https://raw.githubusercontent.com/blawar/titledb/master/cnmts.json",
                        VersionsUrl = "https://raw.githubusercontent.com/blawar/titledb/master/versions.json",
                },
                TitleDatabase = "titledb.db",
                LibraryPath = @"n:\roms\prueba",
        };
        
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = mapperConfig.CreateMapper();

        var optionsMonitor = Substitute.For<IOptionsMonitor<AppSettings>>();
        optionsMonitor.CurrentValue.Returns(appSettings);
        
        _titleLibraryService = new TitleLibraryService(_dataService, _fileInfoService, _titleDbService, optionsMonitor, mapper, _logger);
    }
    
    [Fact]
    public async Task Should_Get_Files()
    {
        //Arrange
        var testFiles = new List<string> { "test1.nsp", "test2.nsz" };
        _fileInfoService.GetFileNames(Arg.Any<string>()).Returns(testFiles);
        
        //Act
        var result = await _titleLibraryService.GetFilesAsync();
        
        //Assert
        Assert.Equal(testFiles, result);
    }

    [Fact]
    public async Task Should_Get_Files_Not_In_Library()
    {
        //Arrange
        var fixture = new Fixture();
        var fixtureLibraryTitles = fixture.CreateMany<LibraryTitle>(5);
        var testFiles = new List<string> { "file1.nsp", "file4.nsp", "file5.nsp", "tango.nsp" };
        var libraryTitles = fixtureLibraryTitles.ToList();
        
        for (var i = 0; i < libraryTitles.Count; i++)
        {
            libraryTitles[i].FileName = $"file{i}.nsp";
        }
        
        _fileInfoService.GetFileNames(Arg.Any<string>()).Returns(testFiles);
        _dataService.GetLibraryTitlesAsync().Returns(libraryTitles);
        
        //Act
        var result = await _titleLibraryService.GetDeltaFilesInLibraryAsync();
        
        //Assert
        Assert.Equal(new List<string> { "file5.nsp", "tango.nsp" }, result.filesToAdd);
        Assert.Equal(libraryTitles.Where(x => x.FileName is "file0.nsp" or "file2.nsp" or "file3.nsp").Select(x => x.TitleId), result.titlesToRemove);
    }
    
    [Fact]
    public async Task ProcessFileAsync_ReturnsLibraryTitle_WhenFileIsValid()
    {
        // Arrange
        var file = "validFile";
        var libraryTitle = new LibraryTitle
        {
                TitleId = "001",
                FileName = "test.nsp"
        };
        var regionTitle = new RegionTitle
        {
                TitleId = "001",
                Name = "Test",
                Region = "US"
        };
        var titleDbCnmt = new List<PackagedContentMeta> { new() { TitleType = 128 } };
        _fileInfoService.GetFileInfo(file, false).Returns(libraryTitle);
        _titleDbService.GetTitle(libraryTitle.TitleId).Returns(regionTitle);
        _titleDbService.GetTitleCnmts(libraryTitle.TitleId).Returns(titleDbCnmt);
        _dataService.AddLibraryTitleAsync(libraryTitle).Returns(Task.CompletedTask);

        // Act
        var result = await _titleLibraryService.ProcessFileAsync(file);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(libraryTitle, result);
    }
}
