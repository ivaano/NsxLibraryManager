using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Services.Interface;
using Radzen;
using Radzen.Blazor;

namespace NsxLibraryManager.Pages;

public partial class Collections : ComponentBase
{
    
    [Inject]
    protected ITitleLibraryService TitleLibraryService { get; set; } = null!;
    private RadzenDataGrid<CollectionDto> _grid = null!;
    private IEnumerable<CollectionDto> _collections = null!;
    private bool _isLoading;
    private readonly IEnumerable<int> _pageSizeOptions = [5, 10, 15, 20, 25];
    private int _pageSize = 5;
    private int _count;

    List<CollectionDto> collectionsToInsert = [];
    List<CollectionDto> collectionsUpdate = [];
    private bool newRecordInsertDisabled = false;
    
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
    
    
    private void Reset()
    {
        collectionsToInsert.Clear();
        collectionsUpdate.Clear();
    }

    private void Reset(CollectionDto collectionDto)
    {
        collectionsToInsert.Remove(collectionDto);
        collectionsUpdate.Remove(collectionDto);
    }

    private async void OnCreateRow(CollectionDto collectionDto)
    {
        try
        {
            newRecordInsertDisabled = false;
            var result = await TitleLibraryService.AddCollection(collectionDto);
            if (result.IsSuccess)
            {
                collectionDto.Id = result.Value.Id;
                await LoadData();
            }
            else
            {
                //notify error
            }
        }
        catch (Exception e)
        {
            //notify error
        }
    }

    private async void OnUpdateRow(CollectionDto collectionDto)
    {
        try
        {
            newRecordInsertDisabled = false;
            Reset(collectionDto);
            var result = await TitleLibraryService.UpdateCollection(collectionDto);
            if (result.IsSuccess)
            {
                await LoadData();
            }
            else
            {
                //notify error
            }
        }
        catch (Exception e)
        {
            throw; //notify error
        }
    }
    
    
    
    private async Task InsertRow()
    {
        if (!_grid.IsValid) return;
        Reset();
        var collection = new CollectionDto();
        newRecordInsertDisabled = true;
        collectionsToInsert.Add(collection);
        await _grid.InsertRow(collection);
    }

    private async Task InsertAfterRow(CollectionDto row)
    {
        if (!_grid.IsValid) return;
        newRecordInsertDisabled = true;
        var collection = new CollectionDto();
        collectionsToInsert.Add(collection);
        await _grid.InsertAfterRow(collection, row);
    }

    private async Task EditRow(CollectionDto collectionDto)
    {
        if (!_grid.IsValid) return;
        Reset();

        collectionsUpdate.Add(collectionDto);
        await _grid.EditRow(collectionDto);
    }

    private async Task DeleteRow(CollectionDto collectionDto)
    {
        Reset(collectionDto);

        if (_collections.Contains(collectionDto))
        {

            var result = await TitleLibraryService.RemoveCollection(collectionDto);
            if (result.IsSuccess)
            {
                await LoadData();
            }
            else
            {
                //notify error
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
    
    void CancelEdit(CollectionDto collectionDto)
    {
        newRecordInsertDisabled = false;
        _grid.CancelEditRow(collectionDto);
/*
        var orderEntry = dbContext.Entry(order);
        if (orderEntry.State == EntityState.Modified)
        {
            orderEntry.CurrentValues.SetValues(orderEntry.OriginalValues);
            orderEntry.State = EntityState.Unchanged;
        }
        */
    }
}