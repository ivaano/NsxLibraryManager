using System.Collections.ObjectModel;
using System.Globalization;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Extensions;

public static class TitleLibraryMappingExtensions
{
    public static LibraryTitleDto MapToLibraryTitleDto(this Title title, 
        IEnumerable<Title>? otherTitles = null, 
        IEnumerable<Models.Titledb.Title>? otherTitlesTitleDb = null)
    {
        var dlcs = new Collection<DlcDto>(
            (otherTitles ?? Array.Empty<Title>())
                .Where(x => x.ContentType == TitleContentType.DLC)
                .Select(d => new DlcDto()
                {
                    ApplicationId = d.ApplicationId,
                    TitleName = d.TitleName,
                    FileName = d.FileName,
                    Version = d.Version,
                    Size = d.Size.GetValueOrDefault(),
                    Screenshots = new Collection<ScreenshotDto>(title.Screenshots.Select(x => new ScreenshotDto()
                    {
                        Url = x.Url
                    }).ToList()),
                }).ToList());
        
        var updates = new Collection<UpdateDto>(
            (otherTitles ?? Array.Empty<Title>())
            .Where(x => x.ContentType == TitleContentType.Update)
            .Select(u => new UpdateDto()
            {
                ApplicationId = u.ApplicationId,
                TitleName = u.TitleName,
                FileName = u.FileName,
                Version = u.Version,
                Size = u.Size.GetValueOrDefault(),
            }).ToList());

        var titleDbDlcs = new Collection<DlcDto>(
            (otherTitlesTitleDb ?? Array.Empty<Models.Titledb.Title>())
            .Where(x => x.ContentType == TitleContentType.DLC)
            .Select(d => new DlcDto()
            {
                ApplicationId = d.ApplicationId,
                TitleName = d.TitleName,
                Size = d.Size.GetValueOrDefault(),
                Screenshots = new Collection<ScreenshotDto>(title.Screenshots.Select(x => new ScreenshotDto()
                {
                    Url = x.Url
                }).ToList()),
            }).ToList());
        
        var titleDbUpdates = new Collection<UpdateDto>(
            (otherTitlesTitleDb ?? Array.Empty<Models.Titledb.Title>())
            .Where(x => x.ContentType == TitleContentType.Update)
            .Select(d => new UpdateDto()
            {
                ApplicationId = d.ApplicationId,
                TitleName = d.TitleName,
                Size = d.Size.GetValueOrDefault()
            }).ToList());
        
        return new LibraryTitleDto
        {
            NsuId = title.NsuId,
            ApplicationId = title.ApplicationId,
            OtherApplicationId = title.OtherApplicationId,
            TitleName = title.TitleName,
            FileName = title.FileName,
            Version = title.Version,
            BannerUrl = title.BannerUrl,
            Description = title.Description,
            ContentType = title.ContentType,
            PackageType = title.PackageType,
            Size = (title.Size is not null) ? (long)title.Size : 0,
            Rating = title.Rating,
            Region = title.Region,
            NumberOfPlayers = title.NumberOfPlayers,
            Dlc = titleDbDlcs,
            Publisher = title.Publisher,
            Updates = titleDbUpdates,
            OwnedDlcs = dlcs,
            OwnedUpdates = updates,
            ReleaseDate = (title.ReleaseDate is not null && title.ReleaseDate != DateTime.MinValue) ? title.ReleaseDate.Value.ToString("MM/dd/yyyy") : string.Empty,
            Versions = new Collection<VersionDto>(
                title.Versions
                    .Select(x => new VersionDto()
            {
                VersionNumber = x.VersionNumber,
                VersionDate = DateOnly.ParseExact(x.VersionDate, "yyyy-MM-dd"),
                ShortVersionNumber = x.VersionNumber.ToString().VersionShifted()
            }).ToList()),
            Screenshots = new Collection<ScreenshotDto>(title.Screenshots.Select(x => new ScreenshotDto()
            {
                Url = x.Url
            }).ToList()),
            Categories = new List<CategoryDto>((title.Categories ?? null).Select(x => new CategoryDto
            {
                Name = x.Name,
            }).ToList()),
        };
    }
}