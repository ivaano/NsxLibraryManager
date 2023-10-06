
namespace NsxLibraryManager.FileLoading.QuickFileInfoLoading;

public interface IPackageInfo
{
    public IEnumerable<IContent>? Contents { get; init; }

    public AccuratePackageType AccuratePackageType { get; init; }

    public PackageType PackageType { get; init; }
}