namespace NsxLibraryManager.Core.FileLoading.Interface;

public interface IPackageInfoLoader
{
    PackageInfo GetPackageInfo(string filePath, bool detailed);
}