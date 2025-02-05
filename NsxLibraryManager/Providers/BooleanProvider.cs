namespace NsxLibraryManager.Providers;

public class BooleanProvider : IFormatProvider, ICustomFormatter
{
    public string Format(string? format, object? arg, IFormatProvider? formatProvider)
    {
        return Equals(arg, true) ? "Yes" : Equals(arg, false) ? "No" : "No value";
    }

    public object? GetFormat(Type? formatType)
    {
        return formatType == typeof(ICustomFormatter) ? this : null;
    }
}