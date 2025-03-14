using Common.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Shared.Settings;

namespace TestUnit.FileLoading;

public class PackageInfoLoaderTest
{
    private readonly IPackageTypeAnalyzer _packageTypeAnalyzer;
    private readonly IKeySetProviderService _keySetProviderService;
    
    public PackageInfoLoaderTest()
    {
        var packageTypelogger = Substitute.For<ILogger<IPackageTypeAnalyzer>>();
        var keySetProviderServiceLogger = Substitute.For<ILogger<IKeySetProviderService>>();
        var settingsMediator = Substitute.For<ISettingsMediator>();
        var appSettings = new UserSettings
        {
            TitleDatabase = "titledb",
            LibraryPath = "library",
            ProdKeys = string.Empty,
            DownloadSettings = new DownloadSettings
            {
                TimeoutInSeconds = 0,
                TitleDbPath = "/tmp",
                TitleDbUrl = "title",
                VersionUrl = "version"
            },
        };
        var options = Options.Create(appSettings);
        _packageTypeAnalyzer = new PackageTypeAnalyzer(packageTypelogger);
        _keySetProviderService = new KeySetProviderService(options, keySetProviderServiceLogger, settingsMediator);
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
        var result = packageInfoLoader.GetPackageInfo(@"N:\roms\prueba\test.nsz", true);
        
        //Assert
        Assert.NotNull(result);
    }    
    
    [Fact ]
    public void Should_Load_Failed_Nsz_FilePackageInfo()
    {
        //Arrange
        var packageInfoLoader = new PackageInfoLoader(_packageTypeAnalyzer, _keySetProviderService);

        //ActAssert
        var result = Assert.Throws<FileNotSupportedException>(() =>  packageInfoLoader.GetPackageInfo(@"N:\roms\prueba\test-fail.nsz", false));
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