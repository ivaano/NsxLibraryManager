using LibHac.Tools.Fs;


namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public class FileContents
{
    public IEnumerable<DirectoryEntryEx>? FileSystemFiles { get; init; }
    public IOrderedEnumerable<Title>? Titles { get; init; }
}