using NsxLibraryManager.Core.Models.Overview;
using NsxLibraryManager.Core.Models.TreeItems.Impl;

namespace NsxLibraryManager.Core.FileLoading.Interface;

public interface IFileOverviewLoader
{
    FileOverview Load(XciItem xciItem);

    FileOverview Load(NspItem nspItem);
}