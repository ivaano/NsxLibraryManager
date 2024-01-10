using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;
using Radzen;

namespace NsxLibraryManager.Pages;

public partial class Settings
{
    
    [Inject] private IOptionsSnapshot<AppSettings> AppSettings { get; set; } = default!;
    [Inject] private IHostApplicationLifetime ApplicationLifetime  { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private bool _recursive = true;
    private IEnumerable<string> _regionsValue = new string[] { "US" };
    private IEnumerable<Region> _regions = new List<Region>() { new() { Name = "US" } };
    private AppSettings _config = default!;
    private bool _databaseFieldDisabled = true;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadConfiguration();
        if (Configuration.GetValue<string>("IsDefaultConfigCreated") == "True")
        {
            _databaseFieldDisabled = false;
            ShowNotification(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Default configuration created", Detail = "A default config.json file has been created, setup the correct paths and restart the application.", Duration = 60000 });
        }
    }
    
    private Task SaveConfiguration()
    {
        var jsonWriteOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        jsonWriteOptions.Converters.Add(new JsonStringEnumConverter());

        var sectionName = new Dictionary<string, AppSettings>
        {
            { AppConstants.AppSettingsSectionName, _config }
        };

        var newJson = JsonSerializer.Serialize(sectionName, jsonWriteOptions);
        var appSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, AppConstants.ConfigFileName);
        File.WriteAllText(appSettingsPath, newJson);
       
        return Task.CompletedTask;
    }
    
    private Task LoadConfiguration()
    {
        _config = AppSettings.Value;
        return Task.CompletedTask;
    }

    private Task ReloadApp()
    {
        ApplicationLifetime.StopApplication();
        return Task.CompletedTask;
    }

    private void ShowNotification(NotificationMessage message)
    {
        NotificationService.Notify(message);
    }
}



public class Region
{
    public string Name { get; set; } = string.Empty;
}