using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;


public class Version : BaseVersion
{
    public required Title Title { get; set; }
}