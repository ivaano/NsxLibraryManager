using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Extensions;

public static class TitleLibraryMappingExtensions
{
    public static LibraryTitleDto MapToLibraryTitleDto(this Title title)
    {
        return new LibraryTitleDto
        {
            NsuId = title.NsuId,
            ApplicationId = title.ApplicationId,
            OtherApplicationId = title.OtherApplicationId,
            TitleName = title.TitleName,
        };
    }
}