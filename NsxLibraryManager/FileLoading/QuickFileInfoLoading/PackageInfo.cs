namespace NsxLibraryManager.FileLoading.QuickFileInfoLoading;

public class PackageInfo : IPackageInfo
{
    public IEnumerable<IContent>? Contents { get; init; }

    public AccuratePackageType AccuratePackageType { get; init; }

    public PackageType PackageType { get; init; }
}

public enum AccuratePackageType
{
    NSP,
    NSZ,
    XCI,
    XCZ
}






