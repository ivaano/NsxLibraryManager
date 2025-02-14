using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.FileLoading.Interface;

public interface IPackageTypeAnalyzer
{
    /// <summary>
    /// Try to detect the real type of the specified file
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    PackageType GetType(string filePath);
}
