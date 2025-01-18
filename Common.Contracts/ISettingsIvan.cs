namespace Common.Contracts;

public interface ISettingsIvan
{
        UserSettingValue GetSettingAsync(string key);
}

public record UserSettingValue(string Key, string Value);