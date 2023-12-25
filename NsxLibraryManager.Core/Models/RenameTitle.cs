namespace NsxLibraryManager.Core.Models;

public record RenameTitle(string SourceFileName, string? DestinationFileName, string? TitleId, string? TitleName, bool Error);