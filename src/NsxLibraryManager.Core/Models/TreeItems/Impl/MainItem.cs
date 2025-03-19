using System;
using LibHac.Loader;
using LibHac.Tools.Fs;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Core.Models.TreeItems.Impl;

public class MainItem : DirectoryEntryItem
{
    public const string MAIN_FILE_NAME = "main";

    public MainItem(NsoHeader nsoHeader, SectionItem parentItem, DirectoryEntryEx directoryEntry) : base(parentItem, directoryEntry)
    {
        ParentItem = parentItem ?? throw new ArgumentNullException(nameof(parentItem));
        NsoHeader = nsoHeader;
    }

    public new SectionItem ParentItem { get; }

    public NsoHeader NsoHeader { get; }

    public sealed override string Format => "Nso";

    public string ModuleId => NsoHeader.ModuleId.Items.ToStrId();

}