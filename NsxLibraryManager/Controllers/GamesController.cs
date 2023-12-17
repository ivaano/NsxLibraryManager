using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Services.Interface;

namespace NsxLibraryManager.Controllers;

public class GamesController(IDataService dataService) : ODataController
{
    [EnableQuery]
    public async Task<ActionResult<IEnumerable<Game>>> Get()
    {
        var libTitles = await dataService.GetLibraryTitlesAsync();

        var games = libTitles.Select(libTitle => new Game
                {
                        TitleId = libTitle.TitleId,
                        FileName = libTitle.FileName,
                        TitleName = libTitle.TitleName,
                        Type = libTitle.Type,
                        TitleVersion = libTitle.TitleVersion,
                        Publisher = libTitle.Publisher
                })
                .ToList();

        return Ok(games);
    }
    
}