using LibHac.Ns;

namespace NsxLibraryManager.Core.FileLoading.QuickFileInfoLoading;

public class NacpData : INacpData
{
    public NacpData(ApplicationControlProperty applicationControlProperty)
    {
        var mask = 1;

        var titles = new Title?[16];

        for (var i = 0; i < 16; i++)
        {
            if ((applicationControlProperty.SupportedLanguageFlag & mask) == mask)
            {
                titles[i] = new Title(applicationControlProperty.Title[i]);
            }
            else
            {
                titles[i] = null;
            }

            mask <<= 1;
        }

        Titles = titles;
    }

    public IReadOnlyList<ITitle?> Titles { get; }
}