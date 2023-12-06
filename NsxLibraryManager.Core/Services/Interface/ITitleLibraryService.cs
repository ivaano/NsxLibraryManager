using NsxLibraryManager.Core.Models;

namespace NsxLibraryManager.Core.Services.Interface;

public interface ITitleLibraryService
{
    public bool DropLibrary();
    public string GetLibraryPath();
    public LibraryTitle? GetTitle(string titleId);
    public Task<LibraryTitle?> GetTitleFromTitleDb(string titleId);
    public Task<LibraryTitle?> ProcessFileAsync(string file);
    public Task<LibraryTitle> DeleteTitleAsync(string titleId);
    public Task ProcessAllTitlesDlc();
    public Task<string> ProcessTitleDlcs(LibraryTitle title);
    public bool AddOwnedDlcsToTitle(LibraryTitle title);

    public Task ProcessAllTitlesUpdates();
    public Task<string> ProcessTitleUpdates(LibraryTitle title);
    public bool AddOwnedUpdatesToTitle(LibraryTitle title);
    public Task<IEnumerable<string>> GetFilesAsync();
    public Task<(IEnumerable<string> filesToAdd, IEnumerable<string> titlesToRemove)>  GetDeltaFilesInLibraryAsync();

}