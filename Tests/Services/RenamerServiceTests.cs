using FluentValidation;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;

namespace Tests.Services;

public class RenamerServiceTests
{
    private readonly RenamerService _renamerService;
    private readonly IFileInfoService _fileInfoService = Substitute.For<IFileInfoService>();
    private readonly IDataService _dataService = Substitute.For<IDataService>();
    private readonly IValidator<RenamerSettings> _validator = Substitute.For<IValidator<RenamerSettings>>();
    private readonly ILogger<RenamerService> _logger = Substitute.For<ILogger<RenamerService>>();
    
    public RenamerServiceTests()
    {
        _renamerService = new RenamerService(_logger, _dataService, _validator, _fileInfoService);
    }
    
    
    [Fact]
    public async Task BuildNewFileNameAsync_ReturnsCorrectPath_WhenPackageTypeIsNspAndTitleTypeIsBase()
    {
        // Arrange
        var filePath = "c:/test/algo.nsp";
        var fileInfo = new LibraryTitle
        {
            PackageType = AccuratePackageType.NSP,
            Type = TitleLibraryType.Base,
            TitleName = "testTitle",
            TitleId = "0010000",
            FileName = "filetest.nsp"
        };
        _fileInfoService.GetFileInfo(filePath, false).Returns(fileInfo);
        var settings = new RenamerSettings
        {
            OutputBasePath = "/home/user/test",
            NspBasePath = "{BasePath}/base/{TitleName}.{Extension}",
            NspUpdatePath = "{BasePath}/updates/{TitleName}.{Extension}",
            NspDlcPath = "{BasePath}/dlc/{TitleName}.{Extension}"
        };
        _dataService.GetRenamerSettings().Returns(settings);

        // Act
        await _renamerService.LoadRenamerSettingsAsync();
        var result = await _renamerService.BuildNewFileNameAsync(fileInfo, filePath);

        // Assert
        Assert.Equal($"{settings.OutputBasePath}/base/{fileInfo.TitleName}.nsp", result);
    }


}