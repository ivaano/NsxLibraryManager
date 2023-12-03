using LibHac.Common.Keys;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;

namespace NsxLibraryManager.Core.FileLoading;

public static class ProcessAppFs
{
    public static FileContents Process(IFileSystem fileSystem, KeySet keySet)
    {
        var fsFiles = fileSystem.EnumerateEntries();
        var switchFs = SwitchFs.OpenNcaDirectory(keySet, fileSystem);
        var titles = switchFs.Titles.Values.OrderBy(x => x.Id);
        
        var fileContents = new FileContents
        {
            FileSystemFiles = fsFiles,
            Titles = titles
        };

        return fileContents;
    }

}