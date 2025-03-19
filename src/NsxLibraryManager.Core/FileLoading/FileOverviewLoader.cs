using System;
using System.Collections.Generic;
using System.Linq;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Models.Overview;
using NsxLibraryManager.Core.Models.TreeItems.Impl;
using ContentType = LibHac.Ncm.ContentType;

namespace NsxLibraryManager.Core.FileLoading;

public class FileOverviewLoader : IFileOverviewLoader
{
    private readonly ILogger _logger;

    public FileOverviewLoader(ILoggerFactory loggerFactory)
    {
        _logger = (loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory))).CreateLogger(this.GetType());
    }

    public FileOverview Load(XciItem xciItem)
    {
        return new FileOverviewLoaderInternal(_logger).CreateXciOverview(xciItem);
    }

    public FileOverview Load(NspItem nspItem)
    {
        return new FileOverviewLoaderInternal(_logger).CreateNspOverview(nspItem);
    }

    private class FileOverviewLoaderInternal
    {
        private readonly ILogger _logger;

        public FileOverviewLoaderInternal(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public FileOverview CreateXciOverview(XciItem xciItem)
        {
            // NOTE: the secure partition of an XCI is equivalent to an NSP
            var securePartitionItem = xciItem.ChildItems.FirstOrDefault(partition => partition.XciPartitionType == XciPartitionType.Secure);

            var fileOverview = new FileOverview(xciItem);

            if (securePartitionItem == null)
            {
                _logger.LogError("XCI secure partition not found!");
                return fileOverview;
            }

            return FillOverview(fileOverview, securePartitionItem);
        }

        public FileOverview CreateNspOverview(NspItem nspItem)
        {
            var fileOverview = new FileOverview(nspItem);
            return FillOverview(fileOverview, nspItem);
        }

        private FileOverview FillOverview(FileOverview fileOverview, PartitionFileSystemItemBase partitionItem)
        {
            var cnmtContainers = BuildCnmtContainers(partitionItem).ToArray();
            fileOverview.CnmtContainers.AddRange(cnmtContainers);
            return fileOverview;
        }


        private IEnumerable<CnmtContainer> BuildCnmtContainers(PartitionFileSystemItemBase partitionItem)
        {

            // Find all Cnmt (kind of manifest containing contents information such a base title, a patch, etc.)
            var cnmtItems = partitionItem.FindAllCnmtItems().ToArray();

            if (cnmtItems.Length <= 0) 
                _logger.LogError("No CNMT entry found!");

            foreach (var cnmtItem in cnmtItems)
            {
                var cnmtContainer = new CnmtContainer(cnmtItem);

                foreach (var cnmtEntryItem in cnmtItem.ChildItems)
                {
                    var referencedNcaItem = cnmtEntryItem.FindReferencedNcaItem();

                    // Search for the NCA referenced by CNMT
                    if (referencedNcaItem == null)
                    {
                        _logger.LogError("NCA entry «{ncaId}» of type «{ncaContentType}» missing.", cnmtEntryItem.NcaId, cnmtEntryItem.NcaContentType);
                        continue;
                    }

                    if (cnmtEntryItem.NcaContentType == ContentType.Control)
                    {
                        var nacpItem = referencedNcaItem.FindNacpItem();
                        if (nacpItem == null)
                            _logger.LogError("NACP file «{fileName}» not found!", NacpItem.NACP_FILE_NAME);
                        else
                            cnmtContainer.NacpContainer = LoadContentDetails(nacpItem);
                    }

                    if (cnmtEntryItem.NcaContentType == ContentType.Program)
                    {
                        // Search for corresponding Program (Code) section
                        var programSection = referencedNcaItem.ChildItems.FirstOrDefault(section => section.NcaSectionType == NcaSectionType.Code);

                        if (programSection == null)
                        {
                            _logger.LogError("NCA of content type «{ncaContentType}» is missing section of type «{code}».",  cnmtEntryItem.NcaContentType, NcaSectionType.Code);
                        }
                        else
                        {
                            cnmtContainer.MainItemSectionIsSparse = programSection.IsSparse;

                            if (!programSection.IsSparse)
                            {
                                var mainItem = programSection.FindChildrenOfType<MainItem>(includeItem: false).FirstOrDefault();
                                cnmtContainer.MainItem = mainItem;

                                if (mainItem == null)
                                    _logger.LogError("File «{fileName}» not found!",  MainItem.MAIN_FILE_NAME);
                            }
                            else
                            {
                                cnmtContainer.MainItem = null;
                            }
                        }

                    }
                }

                yield return cnmtContainer;
            }

        }

        private NacpContainer LoadContentDetails(NacpItem nacpItem)
        {
            var contentDetails = new NacpContainer(nacpItem);

            var nacp = nacpItem.Nacp;

            var language = -1;
            foreach (ref var applicationTitle in nacp.Title.Items)
            {
                language++;

                if (applicationTitle.NameString.IsEmpty())
                    continue;

                var titleInfo = new TitleInfo(ref applicationTitle, (NacpLanguage)language);

                titleInfo.Icon = LoadExpectedIcon(nacpItem.ContainerSectionItem, titleInfo.Language);
                contentDetails.Titles.Add(titleInfo);
            }

            return contentDetails;
        }

        private byte[]? LoadExpectedIcon(SectionItem sectionItem, NacpLanguage nacpLanguage)
        {
            var languageName = nacpLanguage.ToString();

            var expectedFileName = $"icon_{languageName}.dat";

            var iconItem = sectionItem.ChildItems.FirstOrDefault(item => string.Equals(item.Name, expectedFileName, StringComparison.OrdinalIgnoreCase));
            if (iconItem == null)
            {
                _logger.LogError("Expected icon file «{expectedFileName}» missing.", expectedFileName);
                return null;
            }

            var fileSystem = sectionItem.FileSystem;
            if (fileSystem == null)
                return null;

            try
            {
                using var uniqueRefFile = new UniqueRef<IFile>();

                fileSystem.OpenFile(ref uniqueRefFile.Ref, iconItem.Path.ToU8Span(), OpenMode.Read).ThrowIfFailure();
                var file = uniqueRefFile.Release();

                file.GetSize(out var fileSize).ThrowIfFailure();
                var bytes = new byte[fileSize];
                _ = file.AsStream().Read(bytes);
                return bytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load icon: {message}",  ex.Message);
                return null;
            }
        }

    }

}