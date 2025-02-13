using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NsxLibraryManager.Models.Titledb;

public class Edition : BaseEdition
{
    public ICollection<Screenshot>? Screenshots { get; set; }
    public Title Title { get; set; } = null!;
}