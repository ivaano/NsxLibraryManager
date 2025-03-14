using Common.Services;
using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Services.Interface;

public interface IFtpClientService
{
    public Task<Result<bool>> UploadFile(string localPath, string remotePath, string host, int port);
    
    public Task<Result<List<FtpUploadRequest>>> GetQueuedFiles();
}