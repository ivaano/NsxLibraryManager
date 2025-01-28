using System.Collections.ObjectModel;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.Titledb;

namespace NsxLibraryManager.Extensions;

public static class TitledbMappingExtension
{
    
    public static GridTitle MapToGridTitleDto(this Title title)
    {
        return new GridTitle
        {
            ApplicationId = title.ApplicationId,
            BannerUrl = title.BannerUrl,
            ContentType = title.ContentType,
            Description = title.Description,
            DlcCount = title.DlcCount,
            Size = title.Size ?? 0,
            Intro = title.Intro,
            IconUrl = title.IconUrl,
            LatestVersion = title.LatestVersion,
            NsuId = title.NsuId,
            NumberOfPlayers = title.NumberOfPlayers,
            OtherApplicationId = title.OtherApplicationId,
            Publisher = title.Publisher,
            Rating = title.Rating,
            FileName = string.Empty
        };
    }
    
    public static LibraryTitleDto MapToTitleDto(this Title title)
    {
        return new LibraryTitleDto
        {
            ApplicationId = title.ApplicationId,
            TitleName = title.TitleName,
            BannerUrl = title.BannerUrl,
            ContentType = title.ContentType,
            Description = title.Description,
            DlcCount = title.DlcCount,
            Size = title.Size ?? 0,
            Intro = title.Intro,
            IconUrl = title.IconUrl,
            LatestVersion = title.LatestVersion,
            NsuId = title.NsuId,
            NumberOfPlayers = title.NumberOfPlayers,
            OtherApplicationId = title.OtherApplicationId,
            Publisher = title.Publisher,
            Rating = title.Rating,
            ReleaseDate = (title.ReleaseDate is not null && title.ReleaseDate != DateTime.MinValue) ? title.ReleaseDate.Value.ToString("MM/dd/yyyy") : string.Empty,
            Categories = new List<CategoryDto>((title.Categories ?? null).Select(x => new CategoryDto
            {
                Name = x.Name,
            }).ToList()),
            Languages = new List<LanguageDto>((title.Languages ?? null).Select(x => new LanguageDto
            {
                LanguageCode = x.LanguageCode,
            }).ToList()),
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
        };
    }
    
    public static LibraryTitleDto MapToTitleDtoWithOtherTitles(this Title title, IEnumerable<Title>? otherTitles = null)
    {
        var dlcs = new Collection<DlcDto>(
            (otherTitles ?? Array.Empty<Title>())
            .Where(x => x.ContentType == TitleContentType.DLC)
            .Select(d => new DlcDto()
            {
                ApplicationId = d.ApplicationId,
                TitleName = d.TitleName,
                Size = d.Size.GetValueOrDefault(),
                Version = d.LatestVersion.GetValueOrDefault(),
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
                Size = u.Size.GetValueOrDefault(),
            }).ToList());
        
        return new LibraryTitleDto
        {
            ApplicationId = title.ApplicationId,
            BannerUrl = title.BannerUrl,
            ContentType = title.ContentType,
            Description = title.Description,
            Dlc = dlcs,
            DlcCount = title.DlcCount,
            Size = title.Size ?? 0,
            Intro = title.Intro,
            IconUrl = title.IconUrl,
            LatestVersion = title.LatestVersion,
            NsuId = title.NsuId,
            NumberOfPlayers = title.NumberOfPlayers,
            OtherApplicationId = title.OtherApplicationId,
            Publisher = title.Publisher,
            Rating = title.Rating,
            ReleaseDate = (title.ReleaseDate is not null && title.ReleaseDate != DateTime.MinValue) ? title.ReleaseDate.Value.ToString("MM/dd/yyyy") : string.Empty,
            Updates = updates,
            Categories = new List<CategoryDto>((title.Categories ?? null).Select(x => new CategoryDto
            {
                Name = x.Name,
            }).ToList()),
            Languages = new List<LanguageDto>((title.Languages ?? null).Select(x => new LanguageDto
            {
                LanguageCode = x.LanguageCode,
            }).ToList()),
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
        };
    }
}