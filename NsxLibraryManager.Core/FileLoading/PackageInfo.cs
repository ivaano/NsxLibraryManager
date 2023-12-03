using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.FileLoading.Interface;

namespace NsxLibraryManager.Core.FileLoading;

public class PackageInfo : IPackageInfo
{
    public IContent? Contents { get; init; }

    public AccuratePackageType AccuratePackageType { get; init; }

    public PackageType PackageType { get; init; }
}








