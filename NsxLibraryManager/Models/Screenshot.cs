using Microsoft.EntityFrameworkCore;

namespace NsxLibraryManager.Models;

[PrimaryKey("Id")]
public class Screenshot
{
    public int Id { get; init; }
    public string Url { get; init; }
    public int TitleId { get; init; }
    public Title? Title { get; init; }
}