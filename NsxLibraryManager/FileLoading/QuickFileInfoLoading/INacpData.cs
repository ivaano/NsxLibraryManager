namespace NsxLibraryManager.FileLoading.QuickFileInfoLoading;

public interface INacpData
{
    public IReadOnlyList<ITitle?> Titles { get; }
}