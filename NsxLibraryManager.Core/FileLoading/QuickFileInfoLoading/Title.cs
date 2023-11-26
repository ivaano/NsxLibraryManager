using LibHac.Ns;

namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public class Title : ITitle
{
    private readonly ApplicationControlProperty.ApplicationTitle _applicationControlTitle;

    
    public Title(ApplicationControlProperty.ApplicationTitle applicationControlTitle)
    {
        _applicationControlTitle = applicationControlTitle;
    }

    public string? Name => _applicationControlTitle.NameString.ToString();

    public string Publisher => _applicationControlTitle.PublisherString.ToString() ?? string.Empty;
}