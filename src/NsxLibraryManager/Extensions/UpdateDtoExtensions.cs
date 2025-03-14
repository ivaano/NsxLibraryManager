using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Extensions;

public static class UpdateDtoExtensions
{
    public static UpdateDto? GetLatestVersion(this IEnumerable<UpdateDto> updates)
    {
        return updates.OrderByDescending(u => u.Version).FirstOrDefault();
    }
}