using System.Diagnostics;
using Microsoft.Playwright;

namespace TestIntegration;

[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class Tests : PageTest
{
    private IPlaywright _playwright;
    private IBrowser _browser;
    private IBrowserContext _context;
    private IPage _page;
    private readonly string _baseUrl = "http://localhost:5161";
    private Process _appProcess;
    
    [SetUp]
    public async Task SetUp()
    {
        //_appProcess = StartApplication();
        await WaitForApplicationToBeReady();
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, 
        });
            
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }
    
    private static Process StartApplication()
    {
        
        var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "NsxLibraryManager", "NsxLibraryManager.csproj"));
        var startInfo = new ProcessStartInfo("dotnet", $"run --project {projectPath}")
        {
            EnvironmentVariables = {
                { "ASPNETCORE_ENVIRONMENT", "Test" }
            },
            UseShellExecute = false,
            CreateNoWindow = true
        };
        
        var process = Process.Start(startInfo);
        return process ?? throw new InvalidOperationException("Application process could not be started.");
    }
    
    private async Task WaitForApplicationToBeReady()
    {
        using var client = new HttpClient();
        var success = false;
        for (var i = 0; i < 30; i++)
        {
            try
            {
                var response = await client.GetAsync(_baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    success = true;
                    break;
                }
            }
            catch
            {
                // Ignore exceptions, retry.
            }

            await Task.Delay(1000);
        }

        if (!success)
        {
            throw new InvalidOperationException("Application did not start in time.");
        }
    }

    
    [TearDown]
    public async Task TearDown()
    {
        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
        //KillApplicationProcess();
        //DisposeAppProcess();
    }
    
    private void KillApplicationProcess()
    {
        if (_appProcess.HasExited) return;
        try
        {
            _appProcess.Kill();
            _appProcess.WaitForExit(); // Optionally wait for the process to fully exit
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping application: {ex.Message}");
        }
    }

    private void DisposeAppProcess()
    {
        try
        {
            _appProcess.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing process: {ex.Message}");
        }
    }
    
    [Test]
    public async Task HomepageHasTitle()
    {
        await _page.GotoAsync(_baseUrl);
        var title = await _page.TitleAsync();
        Assert.That(title, Does.Contain("NsxLibraryManager"));
    }
    
    [Test]
    public async Task SettingsPageMustShowOnFirstLoad()
    {
        await _page.GotoAsync(_baseUrl);
        await _page.WaitForNavigationAsync();
        var url = _page.Url;
        Assert.Multiple(() =>
        {
            Assert.That(url, Is.Not.Null);
            Assert.That($"{_baseUrl}/settings", Is.EqualTo(url));
        });
    }
    
    [Test]
    public async Task SettingsPageLibraryPathMustShowInvalidPathNotification()
    {
        await _page.GotoAsync($"{_baseUrl}/settings");
        await _page.ClickAsync("#LibraryPath");
        await _page.FillAsync("#LibraryPath", "invalidPath");
        await _page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { NameString = "Save Configuration" }).ClickAsync();
        await _page.WaitForSelectorAsync("text=Library path does not exist.");
    }
}