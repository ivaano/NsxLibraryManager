namespace NsxLibraryManager.Models;

public class TitleDbTitle
{
    public required string id { get; set; }
    public List<string>? ids { get; set; }
    public string? bannerUrl { get; set; }
    public List<string>? category { get; set; }
    public string? description { get; set; }
    public string? developer { get; set; }
    public string? frontBoxArt { get; set; }
    public string? iconUrl { get; set; }
    public string? intro { get; set; }
    public bool isDemo { get; set; }
    public string? key { get; set; }
    public List<string>? languages { get; set; }
    public string? name { get; set; }
    public long nsuId { get; set; }
    public int? numberOfPlayers { get; set; }
    public string? publisher { get; set; }
    public int? rating { get; set; }
    public List<string>? ratingContent { get; set; }
    public string? region { get; set; }
    public int? releaseDate { get; set; }
    public string? rightsId { get; set; }
    public List<string>? screenshots { get; set; }
    public long? size { get; set; }
    public string? version { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
}