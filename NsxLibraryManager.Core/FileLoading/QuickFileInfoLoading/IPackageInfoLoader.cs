namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public interface IPackageInfoLoader
{
    PackageInfo GetPackageInfo(string filePath);
}