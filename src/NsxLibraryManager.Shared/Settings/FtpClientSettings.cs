namespace NsxLibraryManager.Shared.Settings;

public class FtpClientSettings
{
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 21;
    public string RemotePath { get; set; } = "/";
}