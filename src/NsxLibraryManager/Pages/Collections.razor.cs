using Common.Services;
using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class Collections : ComponentBase
{
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = null!;
    
    [Inject]
    protected NotificationService NotificationService { get; set; } = null!;
    
    private RadzenDataGrid<CollectionDto> _grid = null!;
    private IEnumerable<CollectionDto> _collections = null!;
    private bool _isLoading;
    private readonly IEnumerable<int> _pageSizeOptions = [5, 10, 15, 20, 25];
    private int _pageSize = 5;
    private int _count;


    private bool _newRecordInsertDisabled;

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
    
    private async Task LoadData()
    {
        _isLoading = true;
        var collectionResult = await TitleLibraryService.GetCollections();
        if (collectionResult.IsSuccess)
        {
            _collections = collectionResult.Value;
            _count = _collections.Count();
        } 
        else
        {
            _collections = [];
            _count = 0;
        }
        _isLoading = false;

    }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadData();
    }
    

    private async void OnCreateRow(CollectionDto collectionDto)
    {
        try
        {
            _newRecordInsertDisabled = false;
            var result = await TitleLibraryService.AddCollection(collectionDto);
            if (result.IsSuccess)
            {
                collectionDto.Id = result.Value.Id;
                await LoadData();
                ShowNotification(
                    NotificationSeverity.Success, 
                    "Success Adding Collection", 
                    $"{result.Value.Name} Collection Added");
            }
            else
            {
                CancelEdit(collectionDto);
                ShowNotification(
                    NotificationSeverity.Error, 
                    "Error Adding Collection", 
                    result.Error ?? "Unknown Error");
            }
        }
        catch (Exception e)
        {
            CancelEdit(collectionDto);
            ShowNotification(
                NotificationSeverity.Error, 
                "Error Adding Collection", 
                e.Message);
        }
    }

    private async void OnUpdateRow(CollectionDto collectionDto)
    {
        try
        {
            _newRecordInsertDisabled = false;
            var result = await TitleLibraryService.UpdateCollection(collectionDto);
            if (result.IsSuccess)
            {
                await LoadData();
            }
            else
            {
                CancelEdit(collectionDto);
                ShowNotification(
                    NotificationSeverity.Error, 
                    "Error Adding Collection", 
                    result.Error ?? "Unknown Error");
            }
        }
        catch (Exception e)
        {
            CancelEdit(collectionDto);
            ShowNotification(
                NotificationSeverity.Error, 
                "Error Adding Collection", 
                e.Message);
        }
    }
    
    private async Task InsertRow()
    {
        if (!_grid.IsValid) return;
        var collection = new CollectionDto();
        _newRecordInsertDisabled = true;
        await _grid.InsertRow(collection);
    }

    private async Task InsertAfterRow(CollectionDto row)
    {
        if (!_grid.IsValid) return;
        _newRecordInsertDisabled = true;
        var collection = new CollectionDto();
        await _grid.InsertAfterRow(collection, row);
    }

    private async Task EditRow(CollectionDto collectionDto)
    {
        if (!_grid.IsValid) return;
        await _grid.EditRow(collectionDto);
    }

    private async Task DeleteRow(CollectionDto collectionDto)
    {
        if (_collections.Contains(collectionDto))
        {
            var result = await TitleLibraryService.RemoveCollection(collectionDto);
            if (result.IsSuccess)
            {
                await LoadData();
            }
            else
            {
                CancelEdit(collectionDto);
                ShowNotification(
                    NotificationSeverity.Error, 
                    "Error Adding Collection", 
                    result.Error ?? "Unknown Error");
            }
        }
        else
        {
            _grid.CancelEditRow(collectionDto);
        }

        await _grid.Reload();
    }

    private async Task SaveRow(CollectionDto collectionDto)
    {
        await _grid.UpdateRow(collectionDto);
    }

    private void CancelEdit(CollectionDto collectionDto)
    {
        _newRecordInsertDisabled = false;
        _grid.CancelEditRow(collectionDto);
    }
}