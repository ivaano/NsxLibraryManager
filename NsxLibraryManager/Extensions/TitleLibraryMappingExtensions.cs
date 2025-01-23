using System.Collections.ObjectModel;
using System.Globalization;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Extensions;

public static class TitleLibraryMappingExtensions
{
    public static LibraryTitleDto MapLibraryTitleDtoNoDlcOrUpdates(this Title title)
    {
        return new LibraryTitleDto
        {
            ApplicationId = title.ApplicationId,
            BannerUrl = title.BannerUrl,
            ContentType = title.ContentType,
            Description = title.Description,
            DlcCount = title.DlcCount,
            FileName = title.FileName,
            IconUrl = title.IconUrl,
            LatestOwnedUpdateVersion = title.LatestOwnedUpdateVersion,
            LatestVersion = title.LatestVersion,
            NsuId = title.NsuId,
            NumberOfPlayers = title.NumberOfPlayers,
            OtherApplicationId = title.OtherApplicationId,
            OwnedDlcCount = title.OwnedDlcs ?? 0, 
            OwnedUpdatesCount = title.OwnedUpdates ?? 0,
            PackageType = title.PackageType,
            Publisher = title.Publisher,
            Rating = title.Rating,
            Region = title.Region,
            ReleaseDate = (title.ReleaseDate is not null && title.ReleaseDate != DateTime.MinValue) ? title.ReleaseDate.Value.ToString("MM/dd/yyyy") : string.Empty,
            Size = title.Size ?? 0,
            TitleName = title.TitleName,
            UpdatesCount = title.UpdatesCount ?? 0,
            Version = title.Version,
            Categories = (title.Categories is not null) ? new List<CategoryDto>((title.Categories).Select(x => new CategoryDto
                    {
                        Name = x.Name,
                    }).ToList()) : null,
            Languages = new List<LanguageDto>((title.Languages).Select(x => new LanguageDto
                    {
                        LanguageCode = x.LanguageCode,
                    }).ToList()),
            RatingsContent = new List<RatingContentDto>((title.RatingsContents).Select(x => new RatingContentDto()
                    {
                        Name = x.Name,
                    }).ToList()),            
            Screenshots = (title.Screenshots is not null) ? new Collection<ScreenshotDto>(title.Screenshots.Select(x => new ScreenshotDto()
                    {
                        Url = x.Url
                    }).ToList()) : null,
            Versions = (title.Versions is not null) ? new Collection<VersionDto>(
                title.Versions
                    .Select(x => new VersionDto()
                    {
                        VersionNumber = x.VersionNumber,
                        VersionDate = DateOnly.ParseExact(x.VersionDate, "yyyy-MM-dd"),
                        ShortVersionNumber = x.VersionNumber.ToString().VersionShifted()
                    }).ToList()) : null,
        };
    }
    
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
            Languages = new List<LanguageDto>((title.Languages ?? null).Select(x => new LanguageDto
            {
               LanguageCode = x.LanguageCode,
            }).ToList()),            
        };
    }
}