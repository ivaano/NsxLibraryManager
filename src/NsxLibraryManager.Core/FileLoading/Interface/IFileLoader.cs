using NsxLibraryManager.Core.Exceptions;
using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.FileLoading.Interface;

public interface IFileLoader
{
    /// <summary>
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="FileNotSupportedException" />
    public NxFile Load(string filePath);

}