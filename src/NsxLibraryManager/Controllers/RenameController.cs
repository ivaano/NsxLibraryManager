using Microsoft.AspNetCore.Mvc;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/rename")]
public class RenameController : ControllerBase
{
    private readonly ITitleLibraryService _libraryService;
    private readonly IRenamerService _renamerService;
    private readonly ILogger<RenameController> _logger;
    
    
    public RenameController(IRenamerService renamerService, ITitleLibraryService libraryService, ILogger<RenameController> logger)
    {
        _renamerService = renamerService;
        _libraryService = libraryService;
    }
    
    
}