namespace NsxLibraryManager.Shared.Settings;

public class FtpClientSettings
{
    public required string Host { get; set; }
    public int Port { get; set; }
    public required string RemotePath { get; set; }
}