using Microsoft.AspNetCore.Mvc;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Services;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/rename")]
public class RenameController : ControllerBase
{
    private readonly ITitleLibraryService _libraryService;
    private readonly IRenamerService _renamerService;
    private readonly LibraryBackgroundService _backgroundService;
    private readonly LibraryBackgroundStateService _stateService;
    private readonly ILogger<RenameController> _logger;
    private ISettingsService _settingsService;
    
    
    public RenameController(
        IRenamerService renamerService, 
        ITitleLibraryService libraryService,
        LibraryBackgroundStateService stateService,
        IServiceProvider serviceProvider,
        ISettingsService settingsService,
        ILogger<RenameController> logger)
    {
        _renamerService = renamerService;
        _libraryService = libraryService;
        _stateService = stateService;
        _backgroundService = serviceProvider.GetServices<IHostedService>().OfType<LibraryBackgroundService>().First();
        _logger = logger;
        _settingsService = settingsService;
    }
    
    [HttpPost("bundle")]
    [SwaggerOperation(Summary = "Request a bundle rename operation", Description = "Bundle settings must be configured before using this endpoint.")]
    [ProducesResponseType(typeof(LibraryBackgroundResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LibraryBackgroundResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(LibraryBackgroundResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LibraryBackgroundResponse>> RenameBundle()
    {
        var settings = await _settingsService.GetBundleRenamerSettings();
        if (settings.InputPath == string.Empty || settings.OutputBasePath == string.Empty)
        {
            return NotFound("Bundle settings must be configured before using this endpoint.");
        }

        
        var activeTasks = _stateService.GetActiveTasks(LibraryBackgroundTaskType.BundleRename);
        if (activeTasks.Count > 0)
        {
            return StatusCode(StatusCodes.Status409Conflict, new LibraryBackgroundResponse
            {
                TaskId = activeTasks.FirstOrDefault()?.Id,
                Message = $"A rename operation is already in progress.",
                Success = false
            });
        }
        var taskId = _backgroundService.QueueSingleRunLibraryRequest(LibraryBackgroundTaskType.BundleRename);
            
        _logger.LogInformation("Queued {TaskType} task with ID: {TaskId}", LibraryBackgroundTaskType.BundleRename, taskId);
            
        return Ok(new LibraryBackgroundResponse
        {
            TaskId = taskId,
            Message = $"{LibraryBackgroundTaskType.BundleRename} task queued successfully",
            Success = true
        });
    }
    
    
    
    
}