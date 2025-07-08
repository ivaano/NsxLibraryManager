using Microsoft.AspNetCore.Mvc;
using NsxLibraryManager.Services.Interface;
using Radzen;
using System.Text.RegularExpressions;
using NsxLibraryManager.Contracts;
using Swashbuckle.AspNetCore.Annotations;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly ITitleLibraryService _libraryService;
    //private FilterDescriptor? _filterDescriptor;
    private const string ApplicationIdPattern = "^[0-9A-F]{16}$";
    private static readonly RegexOptions RegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;

    public SearchController(ITitleLibraryService libraryService)
    {
        _libraryService = libraryService;
    }
    
    [HttpPost("")]
    [SwaggerOperation(Summary = "Search titles", Description = "Search Titles by TitleId or Name")]
    [ProducesResponseType(typeof(List<SearchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SearchTitles(SearchRequest searchRequest)
    {
        var filterContentType = new FilterDescriptor
        {
            Property = "ContentType",
            FilterOperator = FilterOperator.Equals,
            FilterValue = searchRequest.TitleType,
            LogicalFilterOperator = LogicalFilterOperator.And
        };
        
        var filterSearchDescriptor = new FilterDescriptor
        {
            Property = Regex.IsMatch(searchRequest.NameOrTitleId, ApplicationIdPattern, RegexFlags) 
                ? "ApplicationId" 
                : "TitleName",
            FilterOperator = Regex.IsMatch(searchRequest.NameOrTitleId, ApplicationIdPattern, RegexFlags)
                ? FilterOperator.Equals 
                : FilterOperator.Contains,
            FilterValue = searchRequest.NameOrTitleId
        };
        
        var loadDataArgs = new LoadDataArgs
        {
            Top = searchRequest.PageSize,
            Skip = (searchRequest.Page - 1) * searchRequest.PageSize,
            Filters = [filterContentType, filterSearchDescriptor],
            OrderBy = "TitleName"
        };
        
        var loadData = await _libraryService.GetTitles(loadDataArgs);
        
        if (loadData.IsFailure)
        {
            var errors = new List<string> { loadData.Error ?? "Unknown error"};
            return BadRequest(ErrorResponse.FromError("Failed to process search", errors, "SEARCH_ERROR")  );
        }
        
        var response = new SearchResponse
        {
            Total = loadData.Value.Count,
            Titles = loadData.Value.Titles,
            Page = searchRequest.Page,
            TotalPages = loadData.Value.Count / searchRequest.PageSize + 1
        };
        return Ok(response);

    }
}