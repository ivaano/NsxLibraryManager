using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;

namespace NsxLibraryManager.FileLoading.QuickFileInfoLoading;

public interface IContent
{
    public ContentMetaType Type { get;}

    public string TitleId { get; }

    public string ApplicationTitleId { get; }

    public string PatchTitleId { get; }

    public TitleVersion MinimumApplicationVersion { get; }

    public NacpData? NacpData { get; set; }

    public TitleVersion Version { get; }

    public int PatchNumber { get; }
}