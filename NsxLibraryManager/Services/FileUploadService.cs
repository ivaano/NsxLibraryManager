using NsxLibraryManager.Models;
using NsxLibraryManager.Services.Interface;

namespace NsxLibraryManager.Services;

public class FileUploadService : IFileUploadService
{
    private readonly long maxFileSize = 10 * 1024 * 1024; 
    
    public async Task<FileUploadResponse> UploadFileAsync(IFormFile? file, string uploadDirectory,
        string[] allowedExtensions)
    {
        try
        {
            if (file is null || file.Length == 0)
                return new FileUploadResponse
                {
                    IsSuccess = false,
                    Message = "No file was provided"
                };

            if (file.Length > maxFileSize)
                return new FileUploadResponse
                {
                    IsSuccess = false,
                    Message = "File size exceeds the limit of 10MB"
                };

            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(allowedExtensions, x => x == fileExtension))
                return new FileUploadResponse
                {
                    IsSuccess = false,
                    Message = "File type is not allowed"
                };

            if (!Directory.Exists(uploadDirectory))
                Directory.CreateDirectory(uploadDirectory);

            var filePath = Path.Combine(uploadDirectory, file.FileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new FileUploadResponse
            {
                IsSuccess = true,
                Message = "File uploaded successfully",
                FileName = file.FileName,
                FilePath = filePath,
                FileSize = file.Length,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            return new FileUploadResponse
            {
                IsSuccess = false,
                Message = $"Error uploading file: {ex.Message}"
            };
        }
    }
}