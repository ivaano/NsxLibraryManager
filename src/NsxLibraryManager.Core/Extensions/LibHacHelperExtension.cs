using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Ns;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using NsxLibraryManager.Core.Models.TreeItems.Impl;

namespace NsxLibraryManager.Core.Extensions;

public static class LibHacHelperExtension
{
    public static int GetPatchNumber(this TitleVersion titleVersion)
    {
        return titleVersion.Minor;
    }
}
