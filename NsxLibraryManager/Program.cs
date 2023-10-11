using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using NsxLibraryManager.FileLoading;
using NsxLibraryManager.FileLoading.QuickFileInfoLoading;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.KeysManagement;
using NsxLibraryManager.Settings;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// configuration.
builder.Services
    .AddOptions<AppSettings>()
    .BindConfiguration("NsxLibraryManager")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddHttpClient();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRadzenComponents();
//nsx services
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddSingleton<ITitleLibraryService, TitleLibraryService>();
builder.Services.AddSingleton<ITitleDbService, TitleDbService>();
builder.Services.AddSingleton<IDownloadService, DownloadService>();
builder.Services.AddSingleton<IFileInfoService, FileInfoService>();
builder.Services.AddSingleton<IKeySetProviderService, KeySetProviderService>();
builder.Services.AddSingleton<IPackageTypeAnalyzer, PackageTypeAnalyzer>();
builder.Services.AddSingleton<IPackageInfoLoader, PackageInfoLoader>();
//radzen services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

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