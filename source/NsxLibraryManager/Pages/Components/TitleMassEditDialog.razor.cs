using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public partial class TitleMassEditDialog : ComponentBase
{
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    
    [Inject]
    protected NotificationService NotificationService { get; set; } = null!;
    [Parameter]
    public IList<LibraryTitleDto> SelectedTitles { get; set; } = null!;
    
    private IEnumerable<CollectionDto> _collections = null!;
    private int _dropdownValue;
    private string _relatedPatchTitle = string.Empty;
    private string _relatedDlcTitle = string.Empty;   
    protected override async Task OnParametersSetAsync()
    {
        var hola = false;
    }
    
    private void ShowNotification(NotificationSeverity severity, string summary, string detail, int duration = 4000)
    {
        NotificationService.Notify(new NotificationMessage
        {
            Severity = severity, 
            Summary = summary, 
            Detail = detail, 
            Duration = duration
        });
    }
    
    private async Task LoadCollectionsData()
    {
        await Task.Yield();
        var collectionsResult = await TitleLibraryService.GetCollections();
        if (collectionsResult.IsSuccess)
        {
            _collections = collectionsResult.Value;
        }
    }
    
    private async Task OnSubmit()
    {
        if (_dropdownValue > 0)
        {
            foreach (var title in SelectedTitles)
            {
                title.Collection = _collections.FirstOrDefault(c => c.Id == _dropdownValue);
            }
        }
        else
        {
            foreach (var title in SelectedTitles)
            {
                title.Collection = null;
            }
        }
        

        var updatedTitles = await TitleLibraryService.UpdateMultipleLibraryTitlesAsync(SelectedTitles);
        if (updatedTitles.IsSuccess)
        {
            ShowNotification(
                NotificationSeverity.Success, 
                "Success Updating Title", 
                $"{updatedTitles.Value} Title(s) Updated");
        }

        DialogService.Close(true);
    }
    
    private void Cancel()
    {
        DialogService.Close(false);
    }
}