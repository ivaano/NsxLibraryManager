using System.Text;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using NsxLibraryManager.Core.FileLoading;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Core.Mapping;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Services;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Services.KeysManagement;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Core.Validators;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using Radzen;
using NsxLibraryManager;

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

builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHttpClient();
builder.Services.AddDbContext<NsxLibraryDbContext>(options =>
    options.UseSqlite(initialConfig.GetSection("NsxLibraryManager:NsxLibraryDbConnection").Value));


builder.Services.AddDbContext<TitledbDbContext>(options =>
    options.UseSqlite(initialConfig.GetSection("NsxLibraryManager:TitledbDBConnection").Value));
//builder.Services.AddScoped<ITitledbDatabaseService, TitledbDatabaseService>();

/*
builder.Services.AddDbContext<TitledbDbContext>(options =>
{
    options.UseSqlite(initialConfig.GetSection("NsxLibraryManager:TitledbDBConnection").Value);
    // options.EnableSensitiveDataLogging(true);
});
*/

//builder.Services.AddRazorPages();
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

//nsx services
if (validatorResult.valid)
{
    builder.Services.AddScoped<ISettingsService, SettingsService>();
    //builder.Services.AddTransient<ITitleDbService, TitleDbService>();
    //builder.Services.AddSingleton<IDataService, DataService>();
    builder.Services.AddTransient<IFileInfoService, FileInfoService>();
    builder.Services.AddTransient<IPackageTypeAnalyzer, PackageTypeAnalyzer>();
    builder.Services.AddTransient<IPackageInfoLoader, PackageInfoLoader>();
    builder.Services.AddTransient<IKeySetProviderService, KeySetProviderService>();
    //builder.Services.AddTransient<ITitleLibraryService, TitleLibraryService>();
    builder.Services.AddTransient<ISqlTitleLibraryService, SqlTitleLibraryService>();
    builder.Services.AddTransient<IDownloadService, DownloadService>();

    builder.Services.AddScoped<ITitledbService, TitledbService>();
    builder.Services.AddScoped<ISqlRenamerService, SqlRenamerService>();
    builder.Services.AddScoped<IValidator<PackageRenamerSettings>, RenamerSettingsValidator>();    
    builder.Services.AddScoped<IValidator<BundleRenamerSettings>, BundleSettingsValidator>();    
    builder.Services.AddScoped<IValidator<UserSettings>, UserSettingsValidator>();    
}

//radzen services
builder.Services.AddRadzenComponents();

var modelBuilder = new ODataConventionModelBuilder();
modelBuilder.EntitySet<Game>("Games");

builder.Services.AddControllers().AddOData(
        options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null).AddRouteComponents(
                "odata",
                modelBuilder.GetEdmModel()));

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
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.EnsureDatabaseMigrated<NsxLibraryDbContext>();

app.Run();    

