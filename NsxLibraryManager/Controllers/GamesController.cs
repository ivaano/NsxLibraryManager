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
    public async Task<ActionResult<IEnumerable<Game>>> Get()
    {
        var libTitles = await _dataService.GetLibraryTitlesAsync();

        var games = new List<Game>();
        
        foreach (var libTitle in libTitles)
        {
            var game = new Game
            {
                TitleId = libTitle.TitleId,
                FileName = libTitle.FileName,
                TitleName = libTitle.TitleName,
                Type = libTitle.Type,
                TitleVersion = libTitle.TitleVersion,
                Publisher = libTitle.Publisher
            };
            games.Add(game);
        }
        return Ok(games);
    }
    
}