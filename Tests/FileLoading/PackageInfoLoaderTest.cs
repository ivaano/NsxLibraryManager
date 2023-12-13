using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Core.Settings;

namespace Tests.FileLoading;

public class PackageInfoLoaderTest
{
    private readonly IPackageTypeAnalyzer _packageTypeAnalyzer;
    private readonly IKeySetProviderService _keySetProviderService;
    
    public PackageInfoLoaderTest()
    {
        var packageTypelogger = Substitute.For<ILogger<IPackageTypeAnalyzer>>();
        var keySetProviderServiceLogger = Substitute.For<ILogger<IKeySetProviderService>>();
        var appSettings = new AppSettings
        {
                TitleDatabase = "titledb",
                LibraryPath = "library",
                ProdKeys = string.Empty,
                DownloadSettings = new DownloadSettings
                {
                        TimeoutInSeconds = 0,
                        TitleDbPath = "/tmp",
                        RegionUrl = string.Empty,
                        CnmtsUrl = string.Empty,
                        VersionsUrl = string.Empty,
                        Regions = new List<string>{"US"}
                }
        };
        var options = Options.Create(appSettings);
        _packageTypeAnalyzer = new PackageTypeAnalyzer(packageTypelogger);
        _keySetProviderService = new KeySetProviderService(options, keySetProviderServiceLogger);
    }

    [Fact]
    public void Should_Load_Nsz_FilePackageInfo()
    {
        //Arrange
        var packageInfoLoader = new PackageInfoLoader(_packageTypeAnalyzer, _keySetProviderService);
        
        //Act
        var result = packageInfoLoader.GetPackageInfo(@"N:\roms\prueba\test.nsz", false);
        
        //Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public void Should_Load_Nsz_FilePackageInfo_WithIcon()
    {
        //Arrange
        var packageInfoLoader = new PackageInfoLoader(_packageTypeAnalyzer, _keySetProviderService);
        
        //Act
        var result = packageInfoLoader.GetPackageInfo(@"N:\roms\testa\Asterix & Obelix Slap Them All! 2 [01003AF01B188000][v0].nsz", true);
        //var result = packageInfoLoader.GetPackageInfo(@"N:\roms\testa\A Little to the Left [0100354017668800][v655360].nsz");
        
        //Assert
        Assert.NotNull(result);
    }    
    
    [Fact]
    public void Should_Load_Failed_Nsz_FilePackageInfo()
    {
        //Arrange
        var packageInfoLoader = new PackageInfoLoader(_packageTypeAnalyzer, _keySetProviderService);
        
        //Act
        var result = packageInfoLoader.GetPackageInfo(@"N:\roms\prueba\test-fail.nsz", false);
        
        //Assert
        Assert.NotNull(result);
    }
    
    [Fact]
    public void Should_Load_Xci_FilePackageInfo()
    {
        //Arrange
        var packageInfoLoader = new PackageInfoLoader(_packageTypeAnalyzer, _keySetProviderService);
        
        //Act
        var result = packageInfoLoader.GetPackageInfo(@"N:\roms\prueba\test.xcz", false);
        
        //Assert
        Assert.NotNull(result);
    }
    
    
}