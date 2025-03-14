using LibHac.Ncm;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.Extensions;

public static class PackageInfoMapperExtension
{
    public static LibraryTitleDto ToLibraryTitleDto(this PackageInfo packageInfo, string filePath)
    {
        if (packageInfo.Contents is null)
            throw new Exception($"No contents found in the package of file {filePath}");
        
        return new LibraryTitleDto
        {
            OtherApplicationId = packageInfo.Contents.ApplicationTitleId,
            PatchTitleId = packageInfo.Contents.PatchTitleId,
            PatchNumber = packageInfo.Contents.PatchNumber,
            ApplicationId = packageInfo.Contents.TitleId ?? string.Empty,
            TitleName = packageInfo.Contents.Name,
            Publisher = packageInfo.Contents.Publisher,
            Version = packageInfo.Contents.Version.Version,
            PackageType = packageInfo.AccuratePackageType,
            FileName = Path.GetFullPath(filePath),
            LastWriteTime = File.GetLastWriteTime(filePath),
            ContentType = packageInfo.Contents.Type switch
            {
                ContentMetaType.Application => TitleContentType.Base,
                ContentMetaType.Patch => TitleContentType.Update,
                ContentMetaType.AddOnContent => TitleContentType.DLC,
                _ => TitleContentType.Unknown
            }
            
        };
    }
}