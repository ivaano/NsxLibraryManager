namespace NsxLibraryManager.Providers;

public class BooleanProvider : IFormatProvider, ICustomFormatter
{
    public string Format(string format, object arg, IFormatProvider formatProvider)
    {
        return object.Equals(arg, true) ? "Yes" : object.Equals(arg, false) ? "No" : "No value";
    }

    public object GetFormat(Type formatType)
    {
        if (formatType == typeof(ICustomFormatter))
        {
            return this;
        }

        return null;
    }
}