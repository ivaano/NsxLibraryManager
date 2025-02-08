using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class DuplicateTitles : ComponentBase
{
    [Inject] private ITitleLibraryService TitleLibraryService { get; set; } = default!; 

    private int selectedTabIndex = 0;
    private RadzenDataGrid<LibraryTitleDto> _duplicateTitlesGrid = default!;
    private IEnumerable<LibraryTitleDto> _duplicateTitles = default!;
    private bool isLoading = false;
    private readonly IEnumerable<int> _pageSizeOptions = new[] { 25, 50, 100 };
    private int _count;
    private async Task LoadFiles()
    {
        try
        {
            isLoading = true;
            var duplicateResult = await TitleLibraryService.GetFirstDuplicateTitles();
            if (duplicateResult.IsSuccess)
            {
                _count = duplicateResult.Value.Count;
                _duplicateTitles = duplicateResult.Value.Titles;
            }
            else
            {
            }
                
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }
}