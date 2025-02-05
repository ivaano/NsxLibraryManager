using Common.Contracts;
using LibHac.Common.Keys;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Shared.Settings;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Core.Services.KeysManagement;

public class KeySetProviderService : IKeySetProviderService
{

    private readonly UserSettings _configuration;
    private readonly object _lock = new();
    private readonly ILogger<IKeySetProviderService> _logger;
    private KeySet? _keySet;
    private readonly ISettingsMediator _settingsMediator;

    public KeySetProviderService(IOptions<UserSettings> configuration, ILogger<IKeySetProviderService> logger, ISettingsMediator settingsMediator)
    {
        _configuration = configuration.Value;
        _logger = logger;
        _settingsMediator = settingsMediator;

        AppDirProdKeysFilePath = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, IKeySetProviderService.DefaultProdKeysFileName);
        AppDirTitleKeysFilePath = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, IKeySetProviderService.DefaultTitleKeysFileName);

        Reset();
    }

    public string AppDirProdKeysFilePath { get; }

    public string AppDirTitleKeysFilePath { get; }

    public string? ActualProdKeysFilePath { get; private set; }

    public string? ActualTitleKeysFilePath { get; private set; }

    public string? ActualConsoleKeysFilePath { get; private set; }

    public KeySet GetKeySet(bool forceReload = false)
    {
        lock (_lock)
        {
            if (forceReload)
                UnloadCurrentKeySet();
            else if (_keySet is not null)
                return _keySet;

            _keySet = LoadKeySet();
            return _keySet;
        }
    }


    public void Reset()
    {
        UnloadCurrentKeySet();
        UpdateActualProdKeysFilePath();
        UpdateActualTitleKeysFilePath();
        UpdateActualConsoleKeysFilePath();
    }

    private void UnloadCurrentKeySet()
    {
        lock (_lock)
        {
            _keySet = null;
        }
    }

    /// <summary>
    /// Loads the KeySet with 
    /// </summary>
    /// <returns></returns>
    private KeySet LoadKeySet()
    {
        try
        {
            var actualProdKeysFilePath = ActualProdKeysFilePath;
            var actualTitleKeysFilePath = ActualTitleKeysFilePath;
            var actualConsoleKeysFilePath = ActualConsoleKeysFilePath; 
            var keySet = KeySet.CreateDefaultKeySet();

            ExternalKeyReader.ReadKeyFile(keySet, filename: actualProdKeysFilePath,
                titleKeysFilename: actualTitleKeysFilePath, consoleKeysFilename: actualConsoleKeysFilePath);

            return keySet;
        }
        catch (Exception ex)
        {
            Reset();
           throw new Exception(ex.Message);
        }
    }

    private void UpdateActualProdKeysFilePath()
    {
        var findProdKeysFile = FindKeysFile(_configuration.ProdKeys, IKeySetProviderService.DefaultProdKeysFileName);

        if (findProdKeysFile == null)
            _logger.LogWarning("No ProdKeys File Found!");

        ActualProdKeysFilePath = findProdKeysFile;
    }

    private void UpdateActualTitleKeysFilePath()
    {
        ActualTitleKeysFilePath = FindKeysFile("title.keys", IKeySetProviderService.DefaultTitleKeysFileName);
    }

    private void UpdateActualConsoleKeysFilePath()
    {
        ActualConsoleKeysFilePath = FindKeysFile("console.keys", IKeySetProviderService.DefaultConsoleKeysFileName);
    }

    private string? FindKeysFile(string? keysFilePathRawFromSettings, string keysFileName)
    {
        // 1. Check from settings (if defined)
        if (!string.IsNullOrWhiteSpace(keysFilePathRawFromSettings))
        {
            var keysFilePathTemp = keysFilePathRawFromSettings.ToFullPath();
            if (File.Exists(keysFilePathTemp))
                return keysFilePathTemp;
        }

        // 2. Try to load from config app dir
        var appDirKeysFilePath = Path.Combine(PathHelper.CurrentAppDir, AppConstants.ConfigDirectory, keysFileName);
        if (File.Exists(appDirKeysFilePath))
            return appDirKeysFilePath;

        // 3. Check from "userHomeDir/.switch" directory
        var homeUserDir = PathHelper.HomeUserDir;
        if (homeUserDir is not null)
        {
            var homeDirKeysFilePath = Path.Combine(homeUserDir, ".switch", keysFileName).ToFullPath();
            if (File.Exists(homeDirKeysFilePath))
                return homeDirKeysFilePath;
        }
        
        if (keysFileName == "prods.keys")
            _logger.LogWarning("Prod Keys File Not Found {keysFilePathRawFromSettings}", keysFilePathRawFromSettings);

        return null;
    }

}