using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Extensions;

public static class ExportUserDataExtensions
{
    public static Dictionary<string, List<ExportUserDataDto>> GroupByCollection(
        this List<ExportUserDataDto> userData)
    {
        return userData
            .GroupBy(dto => string.IsNullOrEmpty(dto.Collection) ? "NoCollection" : dto.Collection)
            .ToDictionary(
                group => group.Key,
                group => group.ToList()
            );
    }
}