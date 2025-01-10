﻿using System.Collections.ObjectModel;

namespace NsxLibraryManager.Models.Dto;

public record DlcDto()
{
    public required string ApplicationId { get; init; }
    public string? OtherApplicationId { get; init; }
    public string? TitleName { get; init; }
    public int? Version { get; init; }
    public long? Size { get; init; }


    public string? FileName { get; init; }
    public Collection<ScreenshotDto>? Screenshots { get; init; }

};