using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.FileLoading;

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

    public PackageInfo GetPackageInfo(string filePath, bool detailed)
    {
        var keySet = _keySetProviderService.GetKeySet();

        FileContents fileContents;
        switch (_packageTypeAnalyzer.GetType(filePath))
        {
            case PackageType.UNKNOWN:
                throw new FileNotSupportedException("Unable to determine the package type of the file.", filePath);

            case PackageType.XCI:
                fileContents = LoadXciContents(filePath, keySet, detailed);
                break;

            case PackageType.NSP:
                fileContents = LoadNspContents(filePath, keySet, detailed);

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var packageInfo = new PackageInfo
        {
                PackageType = _packageTypeAnalyzer.GetType(filePath),
                AccuratePackageType = fileContents.AccuratePackageType,
                Contents = FileContentsSummary(fileContents)
        };

        return packageInfo;
    }

    private static IContent? FileContentsSummary(FileContents fileContents)
    {
        var application = fileContents.Applications?.FirstOrDefault();
        if (application == null) return null;
        var publisher = string.Empty;

        foreach (ref readonly var desc in application.Nacp.Value.Title.ItemsRo)
        {
            if (desc.PublisherString.IsEmpty()) continue;
            publisher = desc.PublisherString.ToString();
            break;
        }

        var contentMeta = application.AddOnContent.Count != 0
                ? application.AddOnContent.FirstOrDefault()?.Metadata
                : application.Main?.Metadata ?? application.Patch.Metadata;

        if (contentMeta == null) return null;
        var content = new Content(contentMeta)
        {
                Name = application.Name,
                Publisher = publisher,
                Icon = fileContents.Icon
        };

        return content;
    }

    private static FileContents LoadNspContents(string nspFilePath, KeySet keySet, bool detailed)
    {
        var fileContents = new FileContents
        {
                AccuratePackageType = AccuratePackageType.NSP
        };

        using var file = new LocalStorage(nspFilePath, FileAccess.Read);
        using var pfs = new UniqueRef<PartitionFileSystem>();
        using var hfs = new UniqueRef<Sha256PartitionFileSystem>();

        pfs.Reset(new PartitionFileSystem());
        var res = pfs.Get.Initialize(file);
        if (res.IsSuccess())
        {
            var fileSystem = pfs.Get;
            fileContents = ProcessAppFs.Process(fileSystem, keySet, detailed);
            var containsNcz = fileContents.FileSystemFiles is not null &&
                              fileContents.FileSystemFiles.Any(entry =>
                                      entry.Name.EndsWith(".ncz", StringComparison.OrdinalIgnoreCase));
            fileContents.AccuratePackageType = containsNcz ? AccuratePackageType.NSZ : AccuratePackageType.NSP;
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
            fileContents = ProcessAppFs.Process(fileSystem, keySet, detailed);
        }

        return fileContents;
    }

    private static FileContents LoadXciContents(string xciFilePath, KeySet keySet, bool detailed)
    {
        using var file = new LocalStorage(xciFilePath, FileAccess.Read);
        var xci = new Xci(keySet, file);
        var fileContents = new FileContents
        {
                AccuratePackageType = AccuratePackageType.XCI
        };
        if (xci.HasPartition(XciPartitionType.Secure))
        {
            fileContents = ProcessAppFs.Process(xci.OpenPartition(XciPartitionType.Secure), keySet, detailed);
            var containsNcz = fileContents.FileSystemFiles is not null &&
                              fileContents.FileSystemFiles.Any(entry =>
                                      entry.Name.EndsWith(".ncz", StringComparison.OrdinalIgnoreCase));
            fileContents.AccuratePackageType = containsNcz ? AccuratePackageType.XCZ : AccuratePackageType.XCI;
        }

        return fileContents;
    }
}