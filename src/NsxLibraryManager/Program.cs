using System.Diagnostics;
using System.Text;
using Common.Contracts;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Core.Validators;
using FluentValidation;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Settings;
using Radzen;
using IRenamerService = NsxLibraryManager.Services.Interface.IRenamerService;

Console.OutputEncoding = Encoding.UTF8;

// if no config.json file exists, create one with default values
var configFile = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigDirectory, AppConstants.ConfigFileName);

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile(configFile, optional: true, reloadOnChange: false);
var initialConfig = configBuilder.Build();

var validatorResult = AppSettingsValidator.ValidateRootConfig(initialConfig);

if (validatorResult.defaultConfigCreated)
{
    initialConfig = configBuilder.Build();
}

var builder = WebApplication.CreateBuilder(args);

// configuration.
builder.Configuration.AddConfiguration(initialConfig);
builder.Services
    .AddOptions<AppSettings>()
    .Bind(builder.Configuration.GetSection(AppConstants.AppSettingsSectionName))
    .Configure<IConfiguration>((settings, config) =>
    {
        config.GetSection(AppConstants.AppSettingsSectionName).Bind(settings);
    });
await builder.TitleDbDownloader();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<NsxLibraryDbContext>(options =>
    options.UseSqlite(initialConfig.GetSection("NsxLibraryManager:NsxLibraryDbConnection").Value));
builder.Services.AddDataProtection()
    .PersistKeysToDbContext<NsxLibraryDbContext>();
builder.Services.AddDbContext<TitledbDbContext>(options =>
    options.UseSqlite(initialConfig.GetSection("NsxLibraryManager:TitledbDBConnection").Value));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

//nsx services
if (validatorResult.valid)
{
    builder.Services.AddHostedService<FtpBackgroundService>();
    builder.Services.AddSingleton<FtpStateService>();
    builder.Services.AddScoped<ISettingsService, SettingsService>();
    builder.Services.AddScoped<ISettingsMediator, SettingsService>();
    builder.Services.AddTransient<IFileInfoService, FileInfoService>();
    builder.Services.AddTransient<IPackageTypeAnalyzer, PackageTypeAnalyzer>();
    builder.Services.AddTransient<IPackageInfoLoader, PackageInfoLoader>();
    builder.Services.AddTransient<IKeySetProviderService, KeySetProviderService>();
    builder.Services.AddTransient<IStatsService, StatsService>();
    builder.Services.AddTransient<ITitleLibraryService, TitleLibraryService>();
    builder.Services.AddTransient<IFtpClientService, FtpClientService>();
    builder.Services.AddTransient<IDownloadService, DownloadService>();
    builder.Services.AddScoped<ITitledbService, TitledbService>();
    builder.Services.AddScoped<IRenamerService, RenamerService>();
    builder.Services.AddScoped<IFileUploadService, FileUploadService>();
    builder.Services.AddScoped<IValidator<PackageRenamerSettings>, RenamerSettingsValidator>();    
    builder.Services.AddScoped<IValidator<BundleRenamerSettings>, BundleSettingsValidator>();    
    builder.Services.AddScoped<IValidator<CollectionRenamerSettings>, CollectionSettingsValidator>();    
    builder.Services.AddScoped<IValidator<UserSettings>, UserSettingsValidator>();    
}
builder.Services.AddControllersWithViews();

//radzen services
builder.Services.AddRadzenComponents();
builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = AppConstants.ThemeCookie; 
    options.Duration = TimeSpan.FromDays(365);
});

//Static web assets for linux development https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-7.0&tabs=visual-studio#consume-content-from-a-referenced-rcl
if (builder.Environment.IsEnvironment("DeveLinux"))
{
    builder.WebHost.UseStaticWebAssets();
}

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.MapRazorPages();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.MapControllers();

app.EnsureDatabaseMigrated<NsxLibraryDbContext>();


if (app.Environment.IsProduction())
{
    var noBrowser = args.Contains("--no-browser");
    var isWindows = OperatingSystem.IsWindows();
    if (!noBrowser && isWindows)
    {
        app.Lifetime.ApplicationStarted.Register(() =>
        {
            var logger = app.Logger;
            
            var server = app.Services.GetRequiredService<IServer>();
            var addressesFeature = server.Features.Get<IServerAddressesFeature>();
            var serverUrl = string.Empty;
            if (addressesFeature != null && addressesFeature.Addresses.Count != 0) 
            {
                serverUrl = addressesFeature.Addresses.FirstOrDefault(a =>
                             a.StartsWith("https", StringComparison.OrdinalIgnoreCase)) ?? 
                         addressesFeature.Addresses.FirstOrDefault(a =>
                             a.StartsWith("http", StringComparison.OrdinalIgnoreCase));
            }
            else
            {
                logger.LogWarning("No server addresses registered. Unable to open browser.");
                return;
            }

            try
            {
                logger.LogInformation("Opening browser to {Url}", serverUrl);
                Process.Start(new ProcessStartInfo(serverUrl!) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to open browser");
            }
        });
    }
}

app.Run();    

