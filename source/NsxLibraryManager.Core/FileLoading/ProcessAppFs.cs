using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;

namespace NsxLibraryManager.Core.FileLoading;

public static class ProcessAppFs
{
    public static FileContents Process(IFileSystem fileSystem, KeySet keySet, bool detailed = false)
    {
        var fsFiles = fileSystem.EnumerateEntries();
        var switchFs = SwitchFs.OpenNcaDirectory(keySet, fileSystem);
        var titles = switchFs.Titles.Values.OrderBy(x => x.Id);
        var applications = switchFs.Applications.Values.OrderBy(x => x.Name);
        
        var fileContents = new FileContents
        {
            FileSystemFiles = fsFiles,
            Titles = titles,
            Applications = applications,
        };
        
        if (detailed)
            fileContents.Icon = LoadIcon(switchFs);
    
        return fileContents;
    }
    private static byte[]? LoadIcon(SwitchFs switchFs)
    {
        var contentTitles = switchFs.Titles.Values.OrderBy(x => x.Id);
        if (!switchFs.Titles.TryGetValue(contentTitles.First().Id, out var title)) return null;
        {
            if (title.ControlNca is null) return null;
            //Move this to config later
            var languageName = new List<string> { "AmericanEnglish", "BritishEnglish"};
                    
            var romfs = title.ControlNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.ErrorOnInvalid);
            foreach (var lang in languageName)
            {
                var expectedFileName = $"icon_{lang}.dat";
                var iconItem = romfs.EnumerateEntries().FirstOrDefault(x => x.Name == expectedFileName);
                if (iconItem is not null)
                {
                    using var uniqueRefFile = new UniqueRef<IFile>();

                    romfs.OpenFile(ref uniqueRefFile.Ref, iconItem.FullPath.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                    var file = uniqueRefFile.Release();

                    file.GetSize(out var fileSize).ThrowIfFailure();
                    var bytes = new byte[fileSize];
                    file.AsStream().Read(bytes);
                    return bytes;
                }
            }
        }
        return null;
    }
    

}