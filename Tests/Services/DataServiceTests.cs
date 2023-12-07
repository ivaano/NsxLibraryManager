using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Repository.Interface;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using Tests.Fixtures;

namespace Tests.Services;

public class DataServiceTests : IClassFixture<DatabaseFixture>, IDisposable
{
    private IDataService _dataService;

    public DataServiceTests(DatabaseFixture fixture)
    {
        var titleDbLocation = fixture.DbPath;
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
                TitleDatabase = titleDbLocation,
                LibraryPath = @"n:\roms\prueba",
        };
        var optionsConfiguration = Options.Create(appSettings);
        var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        var mapper = mapperConfig.CreateMapper();
        var logger = Substitute.For<ILogger<DataService>>();
        var memoryCache = Substitute.For<IMemoryCache>();
        _dataService = new DataService(optionsConfiguration, mapper, logger, memoryCache);
    }

    
    [Fact]
    public void Should_Get_Region_Repository()
    {
        //Arrange
        const string testRegion = "US";
        
        //Act
        var result = _dataService.RegionRepository(testRegion);
        
        //Assert
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IRegionRepository>(result);
    }
    
    [Fact]
    public void Should_Get_Cnmts_For_Title()
    {
        //Arrange
        const string testTitleId = "0100000000001000";
        
        //Act
        var packagedContentMetas = _dataService.GetTitleDbCnmtsForTitle(testTitleId);

        //Assert
        Assert.NotNull(packagedContentMetas.FirstOrDefault(x => x.OtherApplicationId == testTitleId));
        Assert.IsAssignableFrom<IEnumerable<PackagedContentMeta>>(packagedContentMetas);
    }

    public void Dispose()
    {
        _dataService.Dispose();
        GC.SuppressFinalize(this);
    }
}