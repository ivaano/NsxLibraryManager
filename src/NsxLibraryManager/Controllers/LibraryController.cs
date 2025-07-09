using Microsoft.AspNetCore.Mvc;
using Radzen;
using System.Text.RegularExpressions;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Services;
using NsxLibraryManager.Shared.Enums;
using Swashbuckle.AspNetCore.Annotations;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/library")]
public class LibraryController : ControllerBase
{
    private readonly ITitleLibraryService _libraryService;
    private readonly LibraryBackgroundService _backgroundService;
    private readonly LibraryBackgroundStateService _stateService;
    private const string ApplicationIdPattern = "^[0-9A-F]{16}$";
    private static readonly RegexOptions RegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;
    private readonly ILogger<LibraryController> _logger;
    
    public LibraryController(
        ITitleLibraryService libraryService,
        LibraryBackgroundStateService stateService,
        IServiceProvider serviceProvider,
        ILogger<LibraryController> logger)
    {
        _stateService = stateService;
        _logger = logger;
        _backgroundService = serviceProvider.GetServices<IHostedService>().OfType<LibraryBackgroundService>().First();
        _libraryService = libraryService;
    }
    
    [HttpPost("search")]
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
    
    
    [HttpPost("action/{actionType}")]
    [SwaggerOperation(Summary = "Perform a Library action", Description = "Perform a refresh or reload operation on library titles. Only one active refresh/reload call will be accepted. If an operation is in progress or waiting, no new action will be queued.")]
    [ProducesResponseType(typeof(LibraryBackgroundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LibraryBackgroundResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public ActionResult<LibraryBackgroundResponse> PerformLibraryAction([FromRoute] string actionType)
    {
        if (!Enum.TryParse(actionType, true, out LibraryBackgroundTaskType taskType) || 
            (taskType != LibraryBackgroundTaskType.Refresh && taskType != LibraryBackgroundTaskType.Reload))
        {
            var errors = new List<string> { $"Invalid action type: '{actionType}'. Supported types are 'refresh' or 'reload'." };
            return BadRequest(ErrorResponse.FromError("Invalid action", errors, "INVALID_ACTION_TYPE"));
        }

        try
        {
            var activeTasks = _stateService.GetActiveTasks(LibraryBackgroundTaskType.Refresh, LibraryBackgroundTaskType.Reload);
            if (activeTasks.Count > 0)
            {
                return StatusCode(StatusCodes.Status409Conflict, new LibraryBackgroundResponse
                {
                    TaskId = activeTasks.FirstOrDefault()?.Id,
                    Message = $"A {activeTasks.FirstOrDefault()?.TaskType.ToString().ToLower()} operation is already in progress.",
                    Success = false
                });
            }

            var taskId = _backgroundService.QueueSingleRunLibraryRequest(taskType);
            
            _logger.LogInformation("Queued {TaskType} task with ID: {TaskId}", taskType, taskId);
            
            return Ok(new LibraryBackgroundResponse
            {
                TaskId = taskId,
                Message = $"{taskType} task queued successfully",
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing {TaskType} task", taskType);
            var errors = new List<string> { ex.Message };
            return StatusCode(StatusCodes.Status500InternalServerError,
                ErrorResponse.FromError($"Error queuing {taskType} task", errors, $"{taskType.ToString().ToUpper()}_ERROR"));
        }
    }
}