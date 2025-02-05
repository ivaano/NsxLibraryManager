namespace NsxLibraryManager.Shared.Dto;

public record RenameTitleDto(
    string SourceFileName, 
    string? DestinationFileName,
    string? TitleId, 
    string? TitleName, 
    bool RenamedSuccessfully,
    bool Error,
    string? ErrorMessage);