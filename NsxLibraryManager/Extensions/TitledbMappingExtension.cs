using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.Titledb;

namespace NsxLibraryManager.Extensions;

public static class TitledbMappingExtension
{
    public static LibraryTitleDto MapToTitleDto(this Title title, IEnumerable<Title>? otherTitles = null)
    {
        return new LibraryTitleDto
        {
            ApplicationId = title.ApplicationId,
            BannerUrl = title.BannerUrl,
            ContentType = title.ContentType,
            Description = title.Description,
            DlcCount = title.DlcCount,
            IconUrl = title.IconUrl,
            LatestVersion = title.LatestVersion,
            NsuId = title.NsuId,
            NumberOfPlayers = title.NumberOfPlayers,
            OtherApplicationId = title.OtherApplicationId,
            Publisher = title.Publisher
        };
    }
}