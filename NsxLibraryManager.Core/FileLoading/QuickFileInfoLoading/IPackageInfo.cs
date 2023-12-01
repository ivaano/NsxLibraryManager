
namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public interface IPackageInfo
{
    public IContent? Contents { get; init; }

    public AccuratePackageType AccuratePackageType { get; init; }

    public PackageType PackageType { get; init; }
}