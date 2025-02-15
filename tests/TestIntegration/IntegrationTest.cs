using Microsoft.Playwright;
using Microsoft.AspNetCore.Mvc.Testing;

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

    
    [SetUp]
    public async Task SetUp()
    {
        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, 
        });
            
        _context = await _browser.NewContextAsync();
        _page = await _context.NewPageAsync();
    }
    
    [TearDown]
    public async Task TearDown()
    {
        await _page.CloseAsync();
        await _context.CloseAsync();
        await _browser.CloseAsync();
        _playwright.Dispose();
    }
    
    [Test]
    public async Task HomepageHasTitleAnd()
    {
        await _page.GotoAsync(_baseUrl);
        var title = await _page.TitleAsync();
        Assert.That(title, Does.Contain("NsxLibraryManager"));
    }
}