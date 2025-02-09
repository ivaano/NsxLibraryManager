using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class DuplicateTitles : ComponentBase
{
    [Inject] private ITitleLibraryService TitleLibraryService { get; set; } = default!; 

    private int selectedTabIndex = 0;
    private RadzenDataGrid<LibraryTitleDto> _duplicateBaseTitlesGrid = default!;
    private RadzenDataGrid<LibraryTitleDto> _duplicateUpdatesTitlesGrid = default!;
    private RadzenDataGrid<LibraryTitleDto> _duplicateDlcTitlesGrid = default!;
    private IEnumerable<LibraryTitleDto> _duplicateBaseTitles = default!;
    private IEnumerable<LibraryTitleDto> _duplicateUpdatesTitles = default!;
    private IEnumerable<LibraryTitleDto> _duplicateDlcTitles = default!;
    private bool isLoading = false;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };
    private int _countBaseTitles;
    private int _countUpdatesTitles;
    private int _countDlcTitles;
    private async Task LoadFiles()
    {
        try
        {
            isLoading = true;
            if (selectedTabIndex == 0)
            {
                var duplicateResult = await TitleLibraryService.GetDuplicateTitles(TitleContentType.Base);
                if (duplicateResult.IsSuccess)
                {
                    _countBaseTitles = duplicateResult.Value.Count;
                    _duplicateBaseTitles = duplicateResult.Value.Titles;
                }
                else
                {
                }    
            }
            if (selectedTabIndex == 1)
            {
                var duplicateResult = await TitleLibraryService.GetDuplicateTitles(TitleContentType.Update);
                if (duplicateResult.IsSuccess)
                {
                    _countUpdatesTitles = duplicateResult.Value.Count;
                    _duplicateUpdatesTitles = duplicateResult.Value.Titles;
                }
                else
                {
                }    
            }
            if (selectedTabIndex == 2)
            {
                var duplicateResult = await TitleLibraryService.GetDuplicateTitles(TitleContentType.DLC);
                if (duplicateResult.IsSuccess)
                {
                    _countDlcTitles = duplicateResult.Value.Count;
                    _duplicateDlcTitles = duplicateResult.Value.Titles;
                }
                else
                {
                }    
            }                
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}