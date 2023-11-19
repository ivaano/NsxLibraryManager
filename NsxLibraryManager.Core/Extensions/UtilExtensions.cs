namespace NsxLibraryManager.Core.Extensions;

public static class UtilExtensions
{
    public static string? ConvertNullOrEmptyTo(this string? s, string? value)
    {
        return string.IsNullOrEmpty(s) ? value : s;
    }
}