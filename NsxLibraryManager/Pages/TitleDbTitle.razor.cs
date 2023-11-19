﻿using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Models.Dto;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Utils;

namespace NsxLibraryManager.Pages;

public partial class TitleDbTitle
{
    [Inject] 
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    [Inject]
    protected ITitleDbService TitleDbService { get; set; } = default!;
    [Parameter] 
    public string? TitleId { get; set; }
    public LibraryTitle? LibraryTitle { get; set; }
    
    public IEnumerable<GameVersions> GameVersions { get; set; } = new List<GameVersions>();
    public IEnumerable<Dlc> GameDlcs { get; set; } = new List<Dlc>();
    
    public string HtmlDescription { get; set; } = string.Empty;
    
    public string GameFileSize { get; set; } = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (TitleId != null)
        {
            LibraryTitle = await TitleLibraryService.GetTitleFromTitleDb(TitleId);
            if (LibraryTitle != null)
            {
                var ownedTitle = TitleLibraryService.GetTitle(TitleId);
                if (ownedTitle is not null) LibraryTitle = ownedTitle;
                var sizeInBytes = LibraryTitle.Size ?? 0;
                GameFileSize = sizeInBytes.ToHumanReadableBytes();
                HtmlDescription = new MarkupString(LibraryTitle.Description.Text2Html()).Value;
                GameVersions = TitleDbService.GetVersions(LibraryTitle.TitleId);
                GameDlcs = await TitleDbService.GetTitleDlc(LibraryTitle.TitleId);
            }
        }
    }
}