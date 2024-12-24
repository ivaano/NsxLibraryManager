using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;

[PrimaryKey("Id")]
public class Version
{
    public int Id { get; set; }
    public int VersionNumber { get; set; }
    public string VersionDate { get; set; }
    public int TitleId { get; set; }

    public Title Title { get; set; }
}