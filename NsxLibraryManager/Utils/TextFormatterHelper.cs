using System.Text;

namespace NsxLibraryManager.Utils;

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

    public static string VersionShifted(this string versionString)
    {
        var intVer = Convert.ToUInt32(versionString);
        var shiftedVer = intVer >> 16;
        return shiftedVer.ToString();
    }
}