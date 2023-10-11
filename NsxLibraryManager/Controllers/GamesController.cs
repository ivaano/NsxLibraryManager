using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services;

namespace NsxLibraryManager.Controllers;

public class GamesController : ODataController
{
    private readonly IDataService _dataService;

    public GamesController(IDataService dataService)
    {
        _dataService = dataService;
    }


    
    [EnableQuery]
    public async Task<ActionResult<IEnumerable<LibraryTitle>>> Get()
    {
        var libTitles = await _dataService.GetLibraryTitlesAsync();

        return Ok();
    }
    
}