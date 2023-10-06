﻿using System.ComponentModel.DataAnnotations;

namespace NsxLibraryManager.Settings;

public class AppSettings
{
    [Required]
    public string TitleDatabase { get; set; } = string.Empty;
    [Required]
    public string LibraryPath { get; set; } = string.Empty;
    public string ProdKeys { get; set; } = string.Empty;

}