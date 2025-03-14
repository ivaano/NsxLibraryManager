namespace NsxLibraryManager.Models.Titledb;

public sealed class Screenshot : BaseScreenshot
{
    public Title? Title { get; init; }
    public Edition? Edition { get; init; }
}