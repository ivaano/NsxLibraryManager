namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public interface INacpData
{
    public IReadOnlyList<ITitle?> Titles { get; }
}