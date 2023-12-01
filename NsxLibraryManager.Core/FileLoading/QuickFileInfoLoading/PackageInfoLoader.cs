using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Services.KeysManagement;


namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public class PackageInfoLoader : IPackageInfoLoader
{
    private readonly IPackageTypeAnalyzer _packageTypeAnalyzer;
    private readonly IKeySetProviderService _keySetProviderService;

    public PackageInfoLoader(IPackageTypeAnalyzer packageTypeAnalyzer, IKeySetProviderService keySetProviderService)
    {
        _packageTypeAnalyzer = packageTypeAnalyzer ?? throw new ArgumentNullException(nameof(packageTypeAnalyzer));
        _keySetProviderService =
            keySetProviderService ?? throw new ArgumentNullException(nameof(keySetProviderService));
    }

    public PackageInfo GetPackageInfo(string filePath)
    {
        var keySet = _keySetProviderService.GetKeySet();

        AccuratePackageType accuratePackageType;
        FileContents fileContents;
        //List<Content> contents;
        switch (_packageTypeAnalyzer.GetType(filePath))
        {
            case PackageType.UNKNOWN:
                throw new FileNotSupportedException(filePath);

            case PackageType.XCI:
                fileContents = LoadXciContents(filePath, keySet, out accuratePackageType);
                break;
                
            case PackageType.NSP:
                fileContents = LoadNspContents(filePath, keySet, out accuratePackageType);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var packageInfo = new PackageInfo
        {
            PackageType = _packageTypeAnalyzer.GetType(filePath),
            AccuratePackageType = accuratePackageType,
            Contents = FileContentsSummary(fileContents)
        };

        return packageInfo;
    }

    private static IContent? FileContentsSummary(FileContents fileContents)
    {
        var firstTitle = fileContents.Titles?.FirstOrDefault();
        if (firstTitle == null) return null;
        var publisher = string.Empty;
        foreach (ref readonly var desc in firstTitle.Control.Value.Title.ItemsRo)
        {
            if (desc.PublisherString.IsEmpty()) continue;
            publisher = desc.PublisherString.ToString();
            break;
        }
        
        var content = new Content(firstTitle.Metadata)
        {
                Name = firstTitle.Name,
                Publisher = publisher
        };
        return content;

    }
  
    private static FileContents LoadNspContents(string nspFilePath, KeySet keySet,
        out AccuratePackageType accuratePackageType)
    {
        var fileContents = new FileContents();
        accuratePackageType = AccuratePackageType.NSP;
        using var file = new LocalStorage(nspFilePath, FileAccess.Read);
        using var pfs = new UniqueRef<PartitionFileSystem>();
        using var hfs = new UniqueRef<Sha256PartitionFileSystem>();
        
        pfs.Reset(new PartitionFileSystem());
        var res = pfs.Get.Initialize(file);
        if (res.IsSuccess())
        {
            var fileSystem = pfs.Get;
            fileContents = ProcessAppFs.Process(fileSystem, keySet);
            var containsNcz = fileContents.FileSystemFiles is not null && fileContents.FileSystemFiles.Any(entry => entry.Name.EndsWith(".ncz", StringComparison.OrdinalIgnoreCase));
            accuratePackageType = containsNcz ? AccuratePackageType.NSZ : AccuratePackageType.NSP;

        }        
        else if (!ResultFs.PartitionSignatureVerificationFailed.Includes(res))
        {
            res.ThrowIfFailure();
        }
        else
        {
            // Reading the input as a PartitionFileSystem didn't work. Try reading it as an Sha256PartitionFileSystem
            hfs.Reset(new Sha256PartitionFileSystem());
            res = hfs.Get.Initialize(file);
            if (res.IsFailure())
            {
                if (ResultFs.Sha256PartitionSignatureVerificationFailed.Includes(res))
                {
                    ResultFs.PartitionSignatureVerificationFailed.Value.ThrowIfFailure();
                }

                res.ThrowIfFailure();
            }

            var fileSystem = hfs.Get;
            fileContents = ProcessAppFs.Process(fileSystem, keySet);
        }

        return fileContents;
    }
    
    private static FileContents LoadXciContents(string xciFilePath, KeySet keySet,
        out AccuratePackageType accuratePackageType)
    {
        accuratePackageType = AccuratePackageType.XCI;
        using var file = new LocalStorage(xciFilePath, FileAccess.Read);
        var xci = new Xci(keySet, file);
        var fileContents = new FileContents();
        if (xci.HasPartition(XciPartitionType.Secure))
        {
            fileContents = ProcessAppFs.Process(xci.OpenPartition(XciPartitionType.Secure), keySet);
            var containsNcz = fileContents.FileSystemFiles is not null && fileContents.FileSystemFiles.Any(entry => entry.Name.EndsWith(".ncz", StringComparison.OrdinalIgnoreCase));
            accuratePackageType = containsNcz ? AccuratePackageType.XCZ : AccuratePackageType.XCI;

        }

        return fileContents;
    }
}