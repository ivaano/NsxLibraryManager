using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;

namespace Tests.Services;

public class FileInfoServiceTests
{
    private readonly IFileInfoService _fileInfoService;
    private readonly IPackageInfoLoader _packageInfoLoader;
    private const string TestTitleId = "1111100000000000";
    private const string TestApplicationTitleId = "1111000000000002";
    private const string TestPatchTitleId = "1111000000000003";
    private const string TestTitleName = "Test Title";
    private const string TestPublisher = "Test Publisher";


    public FileInfoServiceTests()
    {
        var titleDbService = Substitute.For<ITitleDbService>();
        _packageInfoLoader = Substitute.For<IPackageInfoLoader>();
        ILogger<FileInfoService> logger = Substitute.For<ILogger<FileInfoService>>();
        _fileInfoService = new FileInfoService(_packageInfoLoader, titleDbService, logger);
    }

    
    [Fact]
    public async Task Should_Return_File_Info()
    {
        //Arrange
        const string fileName = "testFile.nsp";
        var contents = GetBaseContents();
        var packageInfo = GetBasePackageInfo(content: contents);
        _packageInfoLoader.GetPackageInfo(Arg.Any<string>(), false).Returns(packageInfo);
        
        //Act
        var result = await _fileInfoService.GetFileInfo(fileName, false);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(TestTitleId, result.TitleId);
        Assert.Equal(TestApplicationTitleId, result.ApplicationTitleId);
        Assert.Equal(TestPatchTitleId, result.PatchTitleId);
        Assert.Equal(TestTitleName, result.TitleName);
        Assert.Equal(TestPublisher, result.Publisher);
        Assert.Equal(Path.GetFullPath(fileName), result.FileName);
        Assert.Equal(TitleLibraryType.Base, result.Type);
        Assert.Equal(AccuratePackageType.NSP, result.PackageType);
    }

    [Fact]
    public async Task Should_Return_File_Icon()
    {
        //Arrange
        const string fileName = @"N:\roms\prueba\test.nsz";
        var contents = GetBaseContents();
        var packageInfo = GetBasePackageInfo(content: contents);
        _packageInfoLoader.GetPackageInfo(Arg.Any<string>(), true).Returns(packageInfo);
        
        //Act
        var result = await _fileInfoService.GetFileIcon(fileName);
        
        //Assert
        Assert.NotNull(result);

    }
    
    
    [Fact]
    public async Task Should_Throw_Exception_When_No_Contents()
    {
        //Arrange
        var packageInfo = new PackageInfo
        {
                PackageType = PackageType.NSP,
                AccuratePackageType = AccuratePackageType.NSP,
                Contents = null
        };
        _packageInfoLoader.GetPackageInfo(Arg.Any<string>(), false).Returns(packageInfo);
        
        //Act
        var result = await Assert.ThrowsAsync<Exception>(() => _fileInfoService.GetFileInfo("testFile.nsp", false));

        //Assert
        Assert.Equal("No contents found in the package", result.Message);
    }
    
    [Fact]
    public async Task Should_Throw_Exception_When_Is_InvalidFile()
    {
        //Arrange
        const string filePath = "inexistentfile.txt";
        
        //Act
        var result = await Assert.ThrowsAsync<InvalidPathException>(() => _fileInfoService.GetFileNames(filePath));
        
        //Assert
        Assert.Equal($"Invalid path: {filePath}", result.Message);
    }
    
    [Fact]
    public async Task Should_Get_A_SingleFileName()
    {
        //Arrange
        const string filePath = "Assets/nsptest.nsp";

        //Act
        var result = await _fileInfoService.GetFileNames(filePath);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(Path.GetFullPath(filePath), result.First());
    }
    
    [Fact]
    public async Task Should_Get_All_Files_From_Directory()
    {
        //Arrange
        const string filePath = "Assets";

        //Act
        var result = await _fileInfoService.GetFileNames(filePath);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count());
    }
    
    [Fact]
    public async Task Should_Get_Recursive_Files()
    {   
        //Arrange
        const string filePath = "Assets";
        const bool recursive = true;
        
        //Act
        var result = await _fileInfoService.GetFileNames(filePath, recursive);

        //Assert
        Assert.NotNull(result);
        Assert.Equal(4, result.Count());
    }
    
    private static IPackageInfo GetBasePackageInfo(PackageType packageType = PackageType.NSP, 
            AccuratePackageType accuratePackageType = AccuratePackageType.NSP, IContent? content = null)
    {
        content ??= GetBaseContents();
        var packageInfo = new PackageInfo
        {
                PackageType = packageType,
                AccuratePackageType = accuratePackageType,
                Contents = content
        };
        return packageInfo;
    }
    
    private static IContent GetBaseContents(ContentMetaType contentMetaType = ContentMetaType.Application)
    {
        var contents = Substitute.For<IContent>();
        contents.Type.Returns(contentMetaType);
        contents.Version.Returns(new TitleVersion(1, true));
        contents.TitleId.Returns(TestTitleId);
        contents.ApplicationTitleId.Returns(TestApplicationTitleId);
        contents.PatchTitleId.Returns(TestPatchTitleId);
        contents.Name.Returns(TestTitleName);
        contents.Publisher.Returns(TestPublisher);
        contents.Icon.Returns(new byte[] {0x00, 0x01, 0x02});
        return contents;
    }    
}

