using NsxLibraryManager.Data;
using NsxLibraryManager.FileLoading;
using NsxLibraryManager.FileLoading.QuickFileInfoLoading;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.KeysManagement;
using NsxLibraryManager.Settings;
using Radzen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddOptions<AppSettings>()
    .BindConfiguration("NsxLibraryManager")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddRadzenComponents();
builder.Services.AddSingleton<IDataService, DataService>();
builder.Services.AddSingleton<ITitleLibraryService, TitleLibraryService>();
builder.Services.AddSingleton<IFileInfoService, FileInfoService>();
builder.Services.AddSingleton<IPackageTypeAnalyzer, PackageTypeAnalyzer>();
builder.Services.AddSingleton<IPackageInfoLoader, PackageInfoLoader>();

builder.Services.AddSingleton<IKeySetProviderService, KeySetProviderService>();

builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();