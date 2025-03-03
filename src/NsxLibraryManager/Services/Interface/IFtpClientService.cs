using Common.Services;

namespace NsxLibraryManager.Services.Interface;

public interface IFtpClientService
{
    public Task<Result<bool>> UploadFile(string localPath, string remotePath, string host, int port);
}