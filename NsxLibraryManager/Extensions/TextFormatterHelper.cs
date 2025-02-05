using System.Text;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Extensions;

public static class TextFormatterHelper
{
    public static string ToHumanReadableBytes(this long bytes)
    {
        const int unit = 1024;
        if (bytes < unit) return $"{bytes} B";
        var exp = (int) (Math.Log(bytes) / Math.Log(unit));
        var pre = "KMGTPE"[exp - 1];
        return $"{bytes / Math.Pow(unit, exp):F2} {pre}B";
    }
    
    public static string ToDateFormat(this DateTime dateTime)
    {
        return dateTime.ToString("MM/dd/yyyy");
    }

    public static string ListCategories(this IEnumerable<CategoryDto> categories)
    {
        if (categories is null || !categories.Any())
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        foreach (var category in categories)
        {
            sb.Append(category.Name);
            sb.Append(", ");
        }
        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }
    
    public static string ListRatingsContent(this IEnumerable<RatingContentDto>? ratingsContent)
    {
        if (ratingsContent is null || !ratingsContent.Any())
        {
            return string.Empty;
        }
        var sb = new StringBuilder();
        foreach (var rating in ratingsContent)
        {
            sb.Append(rating.Name);
            sb.Append(", ");
        }
        sb.Remove(sb.Length - 2, 2);
        return sb.ToString();
    }
    
    public static string AgeRating(this int? rating, AgeRatingAgency agency) =>
        agency switch
        {
            AgeRatingAgency.Esrb => rating switch
            {
                < 10 => "Everyone",
                >= 10 and < 13 => "Everyone 10+",
                >= 13 and < 17 => "Teen",
                >= 17 => "Mature",
                _ => string.Empty
            },
            AgeRatingAgency.Pegi => rating switch
            {
                < 3 => "PEGI 3",
                >= 3 and < 7 => "PEGI 7",
                >= 7 and < 12 => "PEGI 12",
                >= 12 and < 16 => "PEGI 16",
                >= 16 => "PEGI 18",
                _ => string.Empty
            },
            AgeRatingAgency.Iarc => rating switch
            {
                < 3 => "3+",
                >= 3 and < 7 => "7+",
                >= 7 and < 12 => "12+",
                >= 12 and < 16 => "16+",
                >= 16 => "18+",
                _ => string.Empty
            },
            _ => "Unknown Agency"
        };
    
    public static string ListToString(this IEnumerable<string>? list)
    {
        return list != null ? string.Join(", ", list) : string.Empty;
    }
    
    public static string NullStringToEmpty(this string? text)
    {
        return text ?? string.Empty;
    }
    
    public static string NullImageToEmpty(this string? text)
    {
        return text ?? "images/no-image.jpg";
    }

    public static string Text2Html(this string? text)
    {
        if (text == null) return string.Empty;
        
        var lines = text.Split(
                new[] { "\r\n", "\r", "\n" },
                StringSplitOptions.None
        );
        var sb = new StringBuilder();
        foreach (var line in lines)
        {
            sb.Append($"{line}<br />");
        }
       
        return sb.ToString();
    }

    public static int VersionShifted(this string versionString)
    {
        var intVer = Convert.ToUInt32(versionString);
        var shiftedVer = intVer >> 16;
        return shiftedVer > 0 ? (int) shiftedVer : 0;
    }
}