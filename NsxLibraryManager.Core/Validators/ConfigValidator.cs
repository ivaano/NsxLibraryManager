using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Core.Validators;

public static class ConfigValidator
{
    public static bool Validate(ConfigurationManager configurationManager)
    {
        if (configurationManager["NsxLibraryManager:LibraryPath"] != string.Empty || configurationManager["NsxLibraryManager:LibraryPath"] is not null)
        {
            return true;
        }
        
        return false;
    }
    
    public static bool Validate(IOptions<AppSettings> configAppSettings)
    {
        //builder.Configuration["NsxLibraryManager:LibraryPath"] != string.Empty || builder.Configuration["NsxLibraryManager:LibraryPath"] is not null
        return true;
    }
}