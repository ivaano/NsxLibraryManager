using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using LibHac.Tools.Ncm;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.FileLoading.Interface;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Core.FileLoading;

/// <summary>
/// A Nintendo Switch package (XCI, NSP, ...) can contain a single content or multiple contents (known as SuperXCI or SuperNSP).
/// Here a <see cref="Content"/> is a model aggregating various information based on a <see cref="Cnmt"/> metadata entry.
/// A <see cref="Content"/> is created for each <see cref="Cnmt"/> found in a Nintendo Switch package.
/// </summary>
public class Content : IContent
{
    private readonly Cnmt _cnmt;

    public Content(Cnmt cnmt)
    {
        _cnmt = cnmt ?? throw new ArgumentNullException(nameof(cnmt));
    }

    public ContentMetaType Type => _cnmt.Type;

    public string TitleId => _cnmt.TitleId.ToStrId();
    public string ApplicationTitleId => _cnmt.ApplicationTitleId.ToStrId();
    public string PatchTitleId => _cnmt.PatchTitleId.ToStrId();
    public TitleVersion MinimumApplicationVersion => _cnmt.MinimumApplicationVersion;
    public TitleVersion Version => _cnmt.TitleVersion;
    public int PatchNumber => _cnmt.TitleVersion.GetPatchNumber();
    public string Name { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public byte[]? Icon { get; set; }
}