namespace Common.Contracts;

public interface ISettingsMediator
{
        UserSettingValue GetUserSetting(string key);
}

public record UserSettingValue(string Key, string Value);