﻿using Microsoft.AspNetCore.Mvc;
using NsxLibraryManager.Models;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Controllers;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly IFileUploadService _fileUploadService;
    private readonly ISettingsService _settingsService;

    public UploadController(IFileUploadService fileUploadService, ISettingsService settingsService)
    {
        _fileUploadService = fileUploadService;
        _settingsService = settingsService;

    }
    
    [HttpPost("keys")]
    [ProducesResponseType(typeof(List<FileUploadResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(List<ErrorResponse>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Post(ICollection<IFormFile> files)
    {
        var errors = new List<string>();
        try
        {
            var uploadResult = new List<FileUploadResponse>();
            foreach (var file in files)
            {
                var result = await _fileUploadService.UploadFileAsync(file ,_settingsService.GetConfigFolder());
                if (!result.IsSuccess)
                    errors.Add($"{file.FileName} - {result.Message}");
                uploadResult.Add(result);
            }

            if (errors.Count != 0)
            {
                return BadRequest(ErrorResponse.FromError("Fail to upload file(s)", errors, "UPLOAD_ERROR"));
            }
            
            return Ok(uploadResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    
    [HttpGet("hi")]
    public ActionResult Get()
    {
        return StatusCode(200);
    }
}