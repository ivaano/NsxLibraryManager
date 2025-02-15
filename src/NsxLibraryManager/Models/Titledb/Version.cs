using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models.Titledb;


public class Version : BaseVersion
{
    public Title Title { get; set; }
}