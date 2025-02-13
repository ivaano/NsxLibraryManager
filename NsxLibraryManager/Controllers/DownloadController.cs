using Microsoft.AspNetCore.Mvc;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/download")]
public class DownloadController : ControllerBase
{
    private readonly ISettingsService _settingsService;

    public DownloadController(ISettingsService settingsService)
    {
        _settingsService = settingsService;

    }
    
    [HttpGet("userdata")] 
    public async Task<IActionResult> DownloadUserDataCsv()
    {
        var result = await _settingsService.ExportUserData();

        if (result.IsSuccess)
        {
            return File(result.Value, "text/csv", "nsxlibrary-usersdata.csv");
        }

        return StatusCode(500);
    }
}