namespace NsxLibraryManager.FileLoading.QuickFileInfoLoading;

public interface IPackageInfoLoader
{
    PackageInfo GetPackageInfo(string filePath);
}