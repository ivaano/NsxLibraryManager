using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.FileLoading.Interface;

public interface IPackageInfo
{
    public IContent? Contents { get; init; }

    public AccuratePackageType AccuratePackageType { get; init; }

    public PackageType PackageType { get; init; }
}