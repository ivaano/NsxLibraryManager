using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.FileLoading;

public class PackageInfo : IPackageInfo
{
    public IContent? Contents { get; init; }

    public AccuratePackageType AccuratePackageType { get; init; }

    public PackageType PackageType { get; init; }
}








