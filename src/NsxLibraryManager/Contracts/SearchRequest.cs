using System.ComponentModel;
using Swashbuckle.AspNetCore.Annotations;

namespace NsxLibraryManager.Contracts;

[SwaggerSchema("Search Titles")]
public class SearchRequest
{
    [SwaggerSchema("Title ID or name to search")]
    [DefaultValue("0100000000010000")]
    public required string NameOrTitleId { get; set; }
    
    [SwaggerSchema("Title type in decimal: 128 base, 129 update, 130 DLC")]
    [DefaultValue(128)]
    public required int TitleType { get; set; }
    
    [SwaggerSchema("Page number to retrieve")]
    [DefaultValue(1)]
    public int Page { get; set; }
    
    [SwaggerSchema("Number of records per page")]
    [DefaultValue(50)]
    public int PageSize { get; set; }
}