using Microsoft.AspNetCore.Mvc;
using NsxLibraryManager.Contracts;
using NsxLibraryManager.Services;
using NsxLibraryManager.Shared.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/refresh")]
public class RefreshController : ControllerBase
{
    private readonly LibraryBackgroundService _backgroundService;
    private readonly LibraryBackgroundStateService _stateService;
    private readonly ILogger<RefreshController> _logger;

    public RefreshController(
        LibraryBackgroundStateService stateService,
        IServiceProvider serviceProvider,
        ILogger<RefreshController> logger)
    {
        _stateService = stateService;
        _logger = logger;
        _backgroundService = serviceProvider.GetServices<IHostedService>().OfType<LibraryBackgroundService>().First();

    }
    
    [HttpPost("")]
    [SwaggerOperation(Summary = "Refresh library titles", Description = "Refresh Library Titles, in the background, only one active refresh call will be accepted, if the refresh is in progress or waiting to be executed no new refresh action will be added to the queue.")]
    [ProducesResponseType(typeof(List<LibraryBackgroundResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status500InternalServerError)]
    public ActionResult<LibraryBackgroundResponse> StartDeltaRefresh()
    {
        try
        {
            var taskId = _backgroundService.QueueSingleRunLibraryRequest(LibraryBackgroundTaskType.Refresh);
            
            _logger.LogInformation("Queued delta refresh task with ID: {TaskId}", taskId);
            
            return Ok(new LibraryBackgroundResponse
            {
                TaskId = taskId,
                Message = "Delta refresh task queued successfully",
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queuing delta refresh task");
            return StatusCode(500, new LibraryBackgroundResponse
            {
                Message = "Failed to queue delta refresh task",
                Success = false
            });
        }
    }
}