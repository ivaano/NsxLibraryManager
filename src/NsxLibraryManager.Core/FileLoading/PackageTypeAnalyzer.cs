using System.Text;
using Microsoft.Extensions.Logging;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Shared.Enums;

namespace NsxLibraryManager.Core.FileLoading;

public class PackageTypeAnalyzer : IPackageTypeAnalyzer
{
    private readonly ILogger<IPackageTypeAnalyzer> _logger;

    public PackageTypeAnalyzer(ILogger<IPackageTypeAnalyzer> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public PackageType GetType(string filePath)
    {
        using var fileStream = File.OpenRead(filePath);
        var buffer = new byte[0x104];
        var read = fileStream.Read(buffer);
        if (read >= 4 && Encoding.ASCII.GetString(buffer, 0, 4) == "PFS0")
        {
            CheckExtensionConsistency(filePath, ".nsp", ".nsz");
            return PackageType.NSP;
        }

        if (read >= 0x104 && Encoding.ASCII.GetString(buffer, 0x100, 4) == "HEAD")
        {
            CheckExtensionConsistency(filePath, ".xci", ".xcz");
            return PackageType.XCI;
        }

        return PackageType.UNKNOWN;
    }

    private void CheckExtensionConsistency(string filePath, string expectedExt1, string expectedExt2)
    {
        var fileExtension = Path.GetExtension(filePath);
        if (!string.Equals(fileExtension, expectedExt1, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(fileExtension, expectedExt2, StringComparison.OrdinalIgnoreCase))
            _logger.LogWarning(fileExtension, expectedExt1, expectedExt2);
    }
}