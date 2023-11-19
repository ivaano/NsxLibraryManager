﻿using LibHac.Common;
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
    public static IEnumerable<PartitionFileEntry> FindCnmtEntries(this PartitionFileSystem fileSystem)
    {
        foreach (var partitionFileEntry in fileSystem.Files)
        {
            var fileName = partitionFileEntry.Name;
            if (fileName.EndsWith("cnmt.nca", StringComparison.OrdinalIgnoreCase))
                yield return partitionFileEntry;
        }
    }
    
    
    
    public static IEnumerable<FoundEntry> FindEntriesAmongSections(this Nca nca, string searchPattern, SearchOptions searchOptions = SearchOptions.RecurseSubdirectories | SearchOptions.CaseInsensitive)
    {
        for (var sectionIndex = 0; sectionIndex < NcaItem.MaxSections; sectionIndex++)
        {
            if (!nca.Header.IsSectionEnabled(sectionIndex))
                continue;

            var sectionFileSystem = nca.OpenFileSystem(sectionIndex, IntegrityCheckLevel.ErrorOnInvalid);

            var entries = sectionFileSystem.EnumerateEntries("/", searchPattern, searchOptions);
            foreach (var entry in entries)
            {
                yield return new FoundEntry(sectionFileSystem, entry);
            }
        }
    }
    
    public static IFile LoadFile(this IFileSystem fileSystem, DirectoryEntryEx directoryEntryEx, OpenMode openMode = OpenMode.Read)
    {
        using var uniqueRefFile = new UniqueRef<IFile>();
        fileSystem.OpenFile(ref uniqueRefFile.Ref, directoryEntryEx.FullPath.ToU8Span(), openMode).ThrowIfFailure();
        return uniqueRefFile.Release();
    }
    
    public static int GetPatchNumber(this TitleVersion titleVersion)
    {
        return titleVersion.Minor;
    }
    
    public static Nca? FindNca(this PartitionFileSystem fileSystem, string ncaId, KeySet keySet)
    {
        var partitionFileEntry = fileSystem.Files.FirstOrDefault(entry => entry.Name.StartsWith(ncaId + ".", StringComparison.OrdinalIgnoreCase));
        if (partitionFileEntry == null)
            return null;

        var ncaFile = fileSystem.OpenFile(partitionFileEntry, OpenMode.Read);

        return new Nca(keySet, new FileStorage(ncaFile));
    }
    
    public static ApplicationControlProperty? LoadNacp(this PartitionFileSystem fileSystem, string ncaId, KeySet keySet)
    {
        var nca = fileSystem.FindNca(ncaId, keySet);
        if (nca == null)
            return null;

        var foundEntry = nca.FindEntriesAmongSections("control.nacp").FirstOrDefault();
        if (foundEntry == null)
            return null;

        var (sectionFileSystem, nacpEntry) = foundEntry;

        var nacpFile = sectionFileSystem.LoadFile(nacpEntry);

        var blitStruct = new BlitStruct<ApplicationControlProperty>(1);
        nacpFile.Read(out _, 0, blitStruct.ByteSpan).ThrowIfFailure();

        return blitStruct.Value;
    }
    
    public static IEnumerable<Cnmt> LoadCnmts(this PartitionFileSystem fileSystem, KeySet keySet)
    {
        foreach (var cnmtNcaPartitionFileEntry in fileSystem.FindCnmtEntries())
        {
            var ncaFile = fileSystem.OpenFile(cnmtNcaPartitionFileEntry, OpenMode.Read);

            var nca = new Nca(keySet, new FileStorage(ncaFile));

            if (nca.Header.ContentType != NcaContentType.Meta)// NOTE: this test is normally not necessary because CNMT are always of type NcaContentType.Meta
                continue;


            var cnmtEntries = nca.FindEntriesAmongSections("*.cnmt");

            foreach (var (sectionFileSystem, cnmtEntry) in cnmtEntries)
            {
                var cnmtFile = sectionFileSystem.LoadFile(cnmtEntry);

                var cnmt = new Cnmt(cnmtFile.AsStream());

                yield return cnmt;

            }
        }
    }
}

public class FoundEntry
{
    public FoundEntry(IFileSystem sectionFileSystem, DirectoryEntryEx entry)
    {
        SectionFileSystem = sectionFileSystem;
        Entry = entry;
    }

    public IFileSystem SectionFileSystem { get; }

    public DirectoryEntryEx Entry { get; }


    public void Deconstruct(out IFileSystem sectionFileSystem, out DirectoryEntryEx entry)
    {
        sectionFileSystem = SectionFileSystem;
        entry = Entry;
    }
}