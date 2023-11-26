using LibHac.Common.FixedArrays;
using LibHac.Ncm;
using LibHac.Ns;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using ILogger = Castle.Core.Logging.ILogger;

namespace Tests.Services;

public class FileInfoServiceTests
{
    private readonly IFileInfoService _fileInfoService;
    private readonly IPackageInfoLoader _packageInfoLoader;
    private readonly ITitleDbService _titleDbService;
    private readonly ILogger<FileInfoService> _logger;
    private const string TestTitleId = "1111100000000000";
    private const string TestApplicationTitleId = "1111000000000002";
    private const string TestPatchTitleId = "1111000000000003";


    public FileInfoServiceTests()
    {
        _titleDbService = Substitute.For<ITitleDbService>();
        _packageInfoLoader = Substitute.For<IPackageInfoLoader>();
        _logger = Substitute.For<ILogger<FileInfoService>>();
        _fileInfoService = new FileInfoService(_packageInfoLoader, _titleDbService, _logger);
    }

    
    [Fact]
    public async Task Should_Return_File_Info_From_NcpData()
    {
        //Arrange
        const string fileName = "testFile.nsp";
        var titleName = new Array512<byte>();
        titleName.Items[0] = 0x54;
        titleName.Items[1] = 0x65;
        titleName.Items[2] = 0x73;
        titleName.Items[3] = 0x74;
        
        var titlePublisher = new Array256<byte>();
        titlePublisher.Items[0] = 0x54;
        titlePublisher.Items[1] = 0x65;
        titlePublisher.Items[2] = 0x73;
        titlePublisher.Items[3] = 0x74;
        
        var appControlProperty = new ApplicationControlProperty();
        var appTitle = new ApplicationControlProperty.ApplicationTitle
        {
           Name = titleName,
           Publisher = titlePublisher
        };
        appControlProperty.Title[0] = appTitle;
        appControlProperty.SupportedLanguageFlag = 1;
        
        var nacpData = new NacpData(appControlProperty);
        var contents = GetBaseContents(nacpData: nacpData);
        var packageInfo = GetBasePackageInfo(content: contents);
        _packageInfoLoader.GetPackageInfo(Arg.Any<string>()).Returns(packageInfo);
        
        //Act
        var result = await _fileInfoService.GetFileInfo(fileName);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(TestTitleId, result.TitleId);
        Assert.Equal(TestApplicationTitleId, result.ApplicationTitleId);
        Assert.Equal(TestPatchTitleId, result.PatchTitleId);
        Assert.Equal("Test", result.TitleName);
        Assert.Equal("Test", result.Publisher);
        Assert.Equal(Path.GetFullPath(fileName), result.FileName);
        Assert.Equal(TitleLibraryType.Base, result.Type);
        Assert.Equal(AccuratePackageType.NSP, result.PackageType);
    }
    
    [Fact]
    public async Task Should_Return_FileInfo_When_NcpDataTitles_Is_Not_First()
    {
        //Arrange
        const string fileName = "testFile.nsp";
        var titleName = new Array512<byte>();
        titleName.Items[0] = 0x54;
        titleName.Items[1] = 0x65;
        titleName.Items[2] = 0x73;
        titleName.Items[3] = 0x74;
        
        var titlePublisher = new Array256<byte>();
        titlePublisher.Items[0] = 0x54;
        titlePublisher.Items[1] = 0x65;
        titlePublisher.Items[2] = 0x73;
        titlePublisher.Items[3] = 0x74;
        
        var appControlProperty = new ApplicationControlProperty();
        var appTitle = new ApplicationControlProperty.ApplicationTitle
        {
                Name = titleName,
                Publisher = titlePublisher
        };
        appControlProperty.Title[2] = appTitle;
        appControlProperty.SupportedLanguageFlag = 4;
        
        var nacpData = new NacpData(appControlProperty);
        var contents = GetBaseContents(nacpData: nacpData);
        var packageInfo = GetBasePackageInfo(content: contents);

        _packageInfoLoader.GetPackageInfo(Arg.Any<string>()).Returns(packageInfo);
        
        //Act
        var result = await _fileInfoService.GetFileInfo(fileName);
        
        //Assert
        Assert.NotNull(result);
        Assert.Equal(TestTitleId, result.TitleId);
        Assert.Equal(TestApplicationTitleId, result.ApplicationTitleId);
        Assert.Equal(TestPatchTitleId, result.PatchTitleId);
        Assert.Equal("Test", result.TitleName);
        Assert.Equal("Test", result.Publisher);
        Assert.Equal(Path.GetFullPath(fileName), result.FileName);
        Assert.Equal(TitleLibraryType.Base, result.Type);
        Assert.Equal(AccuratePackageType.NSP, result.PackageType);
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
        _packageInfoLoader.GetPackageInfo(Arg.Any<string>()).Returns(packageInfo);
        
        //Act
        var result = await Assert.ThrowsAsync<Exception>(() => _fileInfoService.GetFileInfo("testFile.nsp"));

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
                Contents = new List<IContent> {content}
        };
        return packageInfo;
    }
    
    private static IContent GetBaseContents(ContentMetaType contentMetaType = ContentMetaType.Application, 
            NacpData? nacpData = null)
    {
        var contents = Substitute.For<IContent>();
        contents.Type.Returns(contentMetaType);
        contents.Version.Returns(new TitleVersion(1, true));
        contents.TitleId.Returns(TestTitleId);
        contents.ApplicationTitleId.Returns(TestApplicationTitleId);
        contents.PatchTitleId.Returns(TestPatchTitleId);
        contents.NacpData.Returns(nacpData);
        return contents;
    }    
}

