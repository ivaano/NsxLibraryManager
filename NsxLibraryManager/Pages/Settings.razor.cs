using System.Text.Json;
using System.Text.Json.Serialization;
using LibHac.FsSrv;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;

namespace NsxLibraryManager.Pages;

public partial class Settings
{
    
    [Inject] private IOptionsSnapshot<AppSettings> AppSettings { get; set; } = default!;
    [Inject] private IHostApplicationLifetime ApplicationLifetime  { get; set; } = default!;
    
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    
    private bool _recursive = true;
    private IEnumerable<string> _regionsValue = new string[] { "US" };
    private IEnumerable<Region> _regions = new List<Region>() { new() { Name = "US" }, new() { Name = "MX" } };
    private AppSettings _config = default!;
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadConfiguration();
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

    private async Task ReloadApp()
    {
        ApplicationLifetime.StopApplication();
        //return Task.CompletedTask;
    }
}



public class Region
{
    public string Name { get; set; } = string.Empty;
}