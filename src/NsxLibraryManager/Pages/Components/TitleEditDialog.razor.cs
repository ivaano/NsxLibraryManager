using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using Radzen;

namespace NsxLibraryManager.Pages.Components;

public partial class TitleEditDialog : ComponentBase
{
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = default!;
    
    [Inject]
    protected DialogService DialogService { get; set; } = default!;
    
    [Inject]
    protected NotificationService NotificationService { get; set; } = null!;
    
    [Parameter]
    public string? TitleId { get; set; }
    
    private LibraryTitleDto _libraryTitleDto = null!;
    private IEnumerable<CollectionDto> _collections = null!;

    private int _dropdownValue;
    private string _relatedPatchTitle = string.Empty;
    private string _relatedDlcTitle = string.Empty;      
    private bool _disableCollectionDropdown;
    private string _disableCollectionDropdownMessage = string.Empty;
    private int _userRating;
    protected override async Task OnParametersSetAsync()
    {
        await LoadTitle();
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
    
    private async Task LoadTitle()
    {
        if (TitleId is null) return;
        var titleResult = await TitleLibraryService.GetTitleByApplicationId(TitleId);
        if (titleResult.IsSuccess)
        {
            _libraryTitleDto = titleResult.Value;
            _dropdownValue = _libraryTitleDto.Collection?.Id ?? 0;
            _userRating = _libraryTitleDto.UserRating;
            if (_libraryTitleDto.ContentType != TitleContentType.Base && _libraryTitleDto.OtherApplicationId is not null)
            {
                var baseTitleResult = await TitleLibraryService.GetTitleByApplicationId(_libraryTitleDto.OtherApplicationId);
                if (baseTitleResult.IsSuccess)
                {
                    var baseTitle = baseTitleResult.Value;                
                    _relatedPatchTitle = $"{baseTitle?.OwnedUpdatesCount} patches";
                    _relatedDlcTitle = $"{baseTitle?.OwnedDlcCount} dlc";
                }
                else
                {
                    _disableCollectionDropdown = true;
                    _disableCollectionDropdownMessage = $"Base Title with Application Id {_libraryTitleDto.OtherApplicationId} not found, collection for this title can't be updated.";
                }
            }
            else
            {
                _relatedPatchTitle = $"{_libraryTitleDto.OwnedUpdatesCount} patches";
                _relatedDlcTitle = $"{_libraryTitleDto.OwnedDlcCount} dlc";
            }
        }
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
            _libraryTitleDto.Collection = _collections.FirstOrDefault(c => c.Id == _dropdownValue);
        }
        else
        {
            _libraryTitleDto.Collection = null;
        }
        _libraryTitleDto.UserRating = _userRating;
      

        var updatedTitles = await TitleLibraryService.UpdateLibraryTitleAsync(_libraryTitleDto);
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