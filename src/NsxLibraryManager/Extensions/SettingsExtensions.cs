namespace NsxLibraryManager.Extensions;

public static class SettingsExtensions
{
    public static string CleanDatabasePath(this string? databasePath)
    {
        const string removeString = "Data Source=";

        if (databasePath is null) return string.Empty;
        
        var index = databasePath.IndexOf(removeString, StringComparison.OrdinalIgnoreCase);
        databasePath = (index < 0)
            ? databasePath
            : databasePath.Remove(index, removeString.Length);
        return databasePath;

    }
}