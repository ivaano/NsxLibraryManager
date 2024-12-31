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
using NsxLibraryManager.Data;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using Radzen;

Console.OutputEncoding = Encoding.UTF8;

// if no config.json file exists, create one with default values
var configFile = Path.Combine(AppContext.BaseDirectory, AppConstants.ConfigDirectory, AppConstants.ConfigFileName);

var configBuilder = new ConfigurationBuilder()
    .AddJsonFile(configFile, optional: true, reloadOnChange: false);
var configurationRoot = configBuilder.Build();

var validatorResult = ConfigValidator.ValidateRootConfig(configurationRoot);

var builder = WebApplication.CreateBuilder(args);
// configuration.
builder.Services
    .AddOptions<AppSettings>()
    .BindConfiguration(AppConstants.AppSettingsSectionName);

builder.Configuration
    .AddJsonFile(configFile,
        optional: true,
        reloadOnChange: true);

builder.Configuration.AddInMemoryCollection(
    new Dictionary<string, string?>
    {
        { "IsConfigValid", validatorResult.valid.ToString() },
        { "IsDefaultConfigCreated", validatorResult.defaultConfigCreated.ToString()}
    });
builder.Services.AddAutoMapper(typeof(MappingProfile));
builder.Services.AddHttpClient();
builder.Services.AddDbContext<TitledbDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("TitledbDBConnection"));
    // options.EnableSensitiveDataLogging(true);
});
builder.Services.AddDbContext<NsxLibraryDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("NsxLibraryDBConnection"));
    // options.EnableSensitiveDataLogging(true);
});
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
//nsx services
if (validatorResult.valid)
{
    builder.Services.AddTransient<ITitleDbService, TitleDbService>();
    builder.Services.AddSingleton<IDataService, DataService>();
    builder.Services.AddTransient<IFileInfoService, FileInfoService>();
    builder.Services.AddTransient<IPackageTypeAnalyzer, PackageTypeAnalyzer>();
    builder.Services.AddTransient<IPackageInfoLoader, PackageInfoLoader>();
    builder.Services.AddTransient<IKeySetProviderService, KeySetProviderService>();
    builder.Services.AddTransient<ITitleLibraryService, TitleLibraryService>();
    builder.Services.AddTransient<ISqlTitleLibraryService, SqlTitleLibraryService>();
    builder.Services.AddTransient<ISettingsService, SettingsService>();
    builder.Services.AddTransient<IDownloadService, DownloadService>();
    builder.Services.AddScoped<ISqlRenamerService, SqlRenamerService>();
    builder.Services.AddScoped<IValidator<PackageRenamerSettings>, RenamerSettingsValidator>();    
    builder.Services.AddScoped<IValidator<AppSettings>, ConfigValidator>();    
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

app.UseRouting();

app.MapControllers();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");


app.Run();    

