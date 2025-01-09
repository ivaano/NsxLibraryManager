using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.NsxLibrary;

public class Version : BaseVersion
{
    public Title Title { get; init; }
}