using Microsoft.AspNetCore.Components;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Services.Interface;
using Radzen;

namespace NsxLibraryManager.Pages.Components;
#nullable disable
public partial class RefreshLibraryProgressDialog : IDisposable
{
    [Inject]
    protected DialogService DialogService { get; set; }
    [Inject]
    protected ISqlTitleLibraryService TitleLibraryService { get; set; }
    [Inject]
    protected ILogger<ReloadLibraryProgressDialog> Logger { get; set; }
    public double ProgressCompleted { get; set; }
    public int FileCount { get; set; }

    private IEnumerable<string> FilesEnumerable { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await DoWork();
        }
    }

    private async Task DoWork()
    {
        await InvokeAsync(
            async () =>
            {
               
                var filesToProcess = await TitleLibraryService.GetDeltaFilesInLibraryAsync();
                FileCount = filesToProcess.TotalFiles;
                foreach (var libraryTitle in filesToProcess.FilesToAdd)
                {
                    var result = await TitleLibraryService.ProcessFileAsync(libraryTitle.FileName);
                    ProgressCompleted++;
                    StateHasChanged();
                }

                foreach (var libraryTitle in filesToProcess.FilesToRemove)
                {
                    var result = await TitleLibraryService.RemoveLibraryTitleAsync(libraryTitle);
                    ProgressCompleted++;
                    StateHasChanged();
                }
                
                
                //FileCount = filesToProcess.Count();
                /*
               var addedTitles = new List<LibraryTitle>();
               foreach (var file in filesToProcess.filesToAdd)
               {
                   var result = await TitleLibraryService.ProcessFileAsync(file);
                   if (result is not null) addedTitles.Add(result);
                   ProgressCompleted++;
                   StateHasChanged();
               }

               foreach (var titleId in filesToProcess.titlesToRemove)
               {
                   var deleteTitle = await TitleLibraryService.DeleteTitleAsync(titleId);
                   if (deleteTitle.Type != TitleLibraryType.Base)
                   {
                       await TitleLibraryService.ProcessTitleUpdates(deleteTitle);
                       await TitleLibraryService.ProcessTitleDlcs(deleteTitle);
                   }

                   ProgressCompleted++;
                   StateHasChanged();
               }

               var titlesUpdated = new List<string>();
               var titlesDlcUpdated = new List<string>();
               foreach (var title in addedTitles)
               {
                   if (!titlesUpdated.Contains(title.TitleId) || !titlesUpdated.Contains(title.ApplicationTitleId))
                   {
                       var updatedTitle = await TitleLibraryService.ProcessTitleUpdates(title);
                       if (!string.IsNullOrEmpty(updatedTitle))
                           titlesUpdated.Add(updatedTitle);
                   }

                   if (!titlesDlcUpdated.Contains(title.TitleId))
                   {
                       var updatedTitle = await TitleLibraryService.ProcessTitleDlcs(title);
                       if (!string.IsNullOrEmpty(updatedTitle))
                           titlesDlcUpdated.Add(updatedTitle);
                   }
                   //ProgressCompleted++;
                   StateHasChanged();
               }
*/
            });
        DialogService.Close();
    }
    
    void IDisposable.Dispose()
    {
        FileCount = 0;
        ProgressCompleted = 0;
        FilesEnumerable = new List<string>();
    }
}