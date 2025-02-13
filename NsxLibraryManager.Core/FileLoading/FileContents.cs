using LibHac.Tools.Fs;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.FileLoading;

public class FileContents
{
    public IEnumerable<DirectoryEntryEx>? FileSystemFiles { get; init; }
    public IOrderedEnumerable<Title>? Titles { get; init; }
    public IOrderedEnumerable<Application>? Applications { get; init; }
    
    public AccuratePackageType AccuratePackageType { get; set; }
    
    public byte[]? Icon { get; set; } = null;
}