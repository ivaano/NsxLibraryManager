using NsxLibraryManager.Models;

namespace NsxLibraryManager.Services.Interface;

public interface IFileUploadService
{
    Task<FileUploadResponse> UploadFileAsync(IFormFile? file, string uploadDirectory, string[] allowedExtensions);

}