using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Services.Interface;
using Version = NsxLibraryManager.Models.NsxLibrary.Version;


namespace NsxLibraryManager.Services;

public class SqlTitleLibraryService : ISqlTitleLibraryService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext;
    private readonly TitledbDbContext _titledbDbContext;
    private readonly IFileInfoService _fileInfoService;
    private readonly UserSettings _configuration;
    private readonly ILogger<SqlTitleLibraryService> _logger;

    
    public SqlTitleLibraryService(
        NsxLibraryDbContext nsxLibraryDbContext,
        TitledbDbContext titledbDbContext,
        IFileInfoService fileInfoService,
        ISettingsService settingsService,
        ILogger<SqlTitleLibraryService> logger)
    {
        _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
        _titledbDbContext = titledbDbContext ?? throw new ArgumentNullException(nameof(titledbDbContext));
        _fileInfoService = fileInfoService;
        _configuration = settingsService.GetUserSettings();
        _logger = logger;


    }
    public async Task<bool> DropLibrary()
    {
        /*
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleRegion");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleCategory");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleLanguages");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM TitleRatingContents");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Cnmts");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Editions");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Versions");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM ScreenShots");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Cnmts'");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Edition'");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'Versions'");
        await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM sqlite_sequence WHERE name = 'ScreenShots'");
        */
        //await _nsxLibraryDbContext.Database.ExecuteSqlAsync($"DELETE FROM Titles");
        await _nsxLibraryDbContext.Screenshots.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.Versions.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.Titles.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.TitleCategory.ExecuteDeleteAsync();

        
        //await _nsxLibraryDbContext.Database.ExecuteSqlRawAsync($"DELETE FROM sqlite_sequence WHERE name = 'Titles'");
        //await _nsxLibraryDbContext.Database.ExecuteSqlRawAsync($"VACUUM");
        //await Task.Delay(1);
        //return _nsxLibraryDbContext.Database.EnsureDeleted();
        return true;
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        var files = await _fileInfoService.GetFileNames(_configuration.LibraryPath,
            _configuration.Recursive);
        return files;
    }

    private async Task<Title> AddTitleCategories(Title nsxLibraryTitle,  Models.Titledb.Title titledbTitle)
    {
        if (titledbTitle.Categories is null) return nsxLibraryTitle;
        foreach (var category in titledbTitle.Categories)
        {
            var libraryCategory = await _nsxLibraryDbContext.Categories
                .Where(c => c.Name == category.Name)
                .FirstOrDefaultAsync();
            
            if (libraryCategory == null)
            {
                libraryCategory = new Category
                {
                    Name = category.Name,
                };
                _nsxLibraryDbContext.Categories.Add(libraryCategory);
            }
            
            var titleCategory = new TitleCategory
            {
                Title = nsxLibraryTitle,
                Category = libraryCategory,
            };
            
            _nsxLibraryDbContext.TitleCategory.Add(titleCategory);
        }
        
        return nsxLibraryTitle;
    }

    private async Task<Title?> AggregateLibraryTitle(LibraryTitle libraryTitle)
    {
        var titledbTitle = await _titledbDbContext
            .Titles
            .Include(c => c.Categories)
            .Include(s => s.Screenshots)
            .Include(v => v.Versions)
            .FirstOrDefaultAsync(t => t.ApplicationId == libraryTitle.TitleId);
        
        var nsxLibraryTitle = new Title
        {
            FileName = libraryTitle.FileName,
            ApplicationId = libraryTitle.TitleId,
        };
        
        if (titledbTitle is null)
        {
            var metaFromFileName = await _fileInfoService.GetFileInfoFromFileName(libraryTitle.FileName);

            nsxLibraryTitle.TitleName = string.IsNullOrEmpty(libraryTitle.TitleName) 
                ? metaFromFileName.TitleName : libraryTitle.TitleName;

            //If no Type try use the filename
            if (libraryTitle.Type == TitleLibraryType.Unknown)
            {
                nsxLibraryTitle.ContentType = metaFromFileName.Type switch
                {
                    TitleLibraryType.Base => TitleContentType.Base,
                    TitleLibraryType.Update => TitleContentType.Update,
                    TitleLibraryType.DLC => TitleContentType.DLC,
                    _ => TitleContentType.Unknown
                };
            }
            else
            {
                nsxLibraryTitle.ContentType = libraryTitle.Type switch
                {
                    TitleLibraryType.Base => TitleContentType.Base,
                    TitleLibraryType.Update => TitleContentType.Update,
                    TitleLibraryType.DLC => TitleContentType.DLC,
                    _ => TitleContentType.Unknown
                };
            }
            
            nsxLibraryTitle.BannerUrl = libraryTitle.BannerUrl;
            nsxLibraryTitle.Description = libraryTitle.Description;
            nsxLibraryTitle.Developer = libraryTitle.Developer;
            nsxLibraryTitle.IconUrl = libraryTitle.IconUrl;
            nsxLibraryTitle.Intro = libraryTitle.Intro;
            nsxLibraryTitle.LastWriteTime = libraryTitle.LastWriteTime;
            nsxLibraryTitle.NsuId = libraryTitle.Nsuid;
            nsxLibraryTitle.NumberOfPlayers = libraryTitle.NumberOfPlayers;
            nsxLibraryTitle.PackageType = metaFromFileName.PackageType;
            nsxLibraryTitle.Publisher = libraryTitle.Publisher;
            nsxLibraryTitle.Rating = libraryTitle.Rating;
            nsxLibraryTitle.ReleaseDate = libraryTitle.ReleaseDate;
            nsxLibraryTitle.Size = metaFromFileName.Size;
            nsxLibraryTitle.Version = (int?)libraryTitle.TitleVersion;
           
            /*            
            if (libraryTitle.Type == TitleLibraryType.Base)
            {
                title.IconUrl = await _fileInfoService.GetFileIcon(libraryTitle.FileName);    
            }
            */
            return nsxLibraryTitle;
        }

        if (titledbTitle.Categories is not null)
        {
            await AddTitleCategories(nsxLibraryTitle, titledbTitle);
        }

        nsxLibraryTitle.BannerUrl = titledbTitle.BannerUrl;
        nsxLibraryTitle.ContentType = titledbTitle.ContentType;
        nsxLibraryTitle.Description = titledbTitle.Description;
        nsxLibraryTitle.Developer = titledbTitle.Developer;
        nsxLibraryTitle.DlcCount = titledbTitle.DlcCount;
        nsxLibraryTitle.DlcCount = titledbTitle.DlcCount;
        nsxLibraryTitle.IconUrl = titledbTitle.IconUrl;
        nsxLibraryTitle.Intro = titledbTitle.Intro;
        nsxLibraryTitle.LastWriteTime = libraryTitle.LastWriteTime;
        nsxLibraryTitle.LatestVersion = titledbTitle.LatestVersion;
        nsxLibraryTitle.NsuId = titledbTitle.NsuId;
        nsxLibraryTitle.NumberOfPlayers = titledbTitle.NumberOfPlayers;
        nsxLibraryTitle.OtherApplicationId = titledbTitle.OtherApplicationId;
        nsxLibraryTitle.PackageType = libraryTitle.PackageType;
        // prefer the publisher from the file
        nsxLibraryTitle.Publisher = titledbTitle.Publisher.ConvertNullOrEmptyTo(libraryTitle.Publisher);
        nsxLibraryTitle.Rating = titledbTitle.Rating;
        nsxLibraryTitle.Region = titledbTitle.Region;
        nsxLibraryTitle.ReleaseDate = titledbTitle.ReleaseDate;
        // prefer size from actual file
        nsxLibraryTitle.Size = await _fileInfoService.GetFileSize(libraryTitle.FileName);
        // prefer the title name from titledb
        nsxLibraryTitle.TitleName = libraryTitle.TitleName.ConvertNullOrEmptyTo(titledbTitle.TitleName);
        nsxLibraryTitle.UpdatesCount = titledbTitle.UpdatesCount;
        nsxLibraryTitle.Version = (int?)libraryTitle.TitleVersion;
       
        if (titledbTitle.Screenshots != null)
        {
            var screenshots = new Collection<Screenshot>();
            foreach (var titleScreenshot in titledbTitle.Screenshots)
            {
                screenshots.Add(new Screenshot
                {
                    Url = titleScreenshot.Url,
                });
            }
            nsxLibraryTitle.Screenshots = screenshots;
        }

        if (titledbTitle.Versions is not null && titledbTitle.Versions.Count > 0)
        {
            var versions = new Collection<Version>();
            foreach (var version in titledbTitle.Versions)
            {
                versions.Add(new Version
                {
                    VersionNumber = version.VersionNumber,
                    VersionDate = version.VersionDate,
                });
            }
            nsxLibraryTitle.Versions = versions;
        }
        
        return nsxLibraryTitle; 
    }

    public async Task<bool> SaveDatabaseChangesAsync()
    {
        await _nsxLibraryDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SaveContentCounts(Dictionary<string, int> updateCounts, TitleContentType titleContentType)
    {
        foreach (var count in updateCounts)
        {
            var title = await _nsxLibraryDbContext.Titles
                .FirstOrDefaultAsync(t => t.ApplicationId == count.Key);
            if (title is not null)
            {
                switch (titleContentType)
                {
                    case TitleContentType.Update:
                        title.OwnedUpdates = count.Value;
                        break;
                    case TitleContentType.DLC:
                        title.OwnedDlcs = count.Value;
                        break;
                }
            }
        }
        await _nsxLibraryDbContext.SaveChangesAsync();
        return true;
    }
    
    public Task<IQueryable<DlcDto>> GetTitleDlcsAsync(string applicationId)
    {
        var query = _titledbDbContext.Titles.AsNoTracking()
            .Where(t => t.OtherApplicationId == applicationId)
            .Where(t => t.ContentType == TitleContentType.DLC)
            .OrderByDescending(t => t.ReleaseDate)
            .Select(t => new DlcDto
            {
                ApplicationId = t.ApplicationId,
                OtherApplicationId = t.OtherApplicationId,
                TitleName = t.TitleName,
                Size = t.Size,
                
            }).AsQueryable();
        return Task.FromResult(query);
    }

    public async Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId)
    {
       
        var title = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Include(v => v.Versions)
            .Include(c => c.Categories)
            .Include(s => s.Screenshots)
            .FirstOrDefaultAsync(t => t.ApplicationId == applicationId);

        var relatedTitles = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.OtherApplicationId == applicationId)
            .OrderByDescending(t => t.ReleaseDate)
            .ToListAsync();
        
        var relatedTitlesTitleDb = await _titledbDbContext.Titles
            .AsNoTracking()
            .Where(t => t.OtherApplicationId == applicationId)
            .OrderByDescending(t => t.ReleaseDate)
            .ToListAsync();
        
        return title.MapToLibraryTitleDto(relatedTitles, relatedTitlesTitleDb);
    }

    public async Task<Title?> ProcessFileAsync(string file)
    {
        try
        {
            _logger.LogDebug("Processing file: {file}", file);
            var libraryTitle = await _fileInfoService.GetFileInfo(file, detailed: false);
            if (libraryTitle is null)
            {
                _logger.LogDebug("Unable to get File Information from file : {file}", file);
                return null;
            }
            var title = await AggregateLibraryTitle(libraryTitle);
            if (title is not null)
            {
                _nsxLibraryDbContext.Add(title);    
            }
            
            await _nsxLibraryDbContext.SaveChangesAsync();
            /*
            var titledbTitle = await _titleDbService.GetTitle(libraryTitle.TitleId);
            var titleDbCnmt = _titleDbService.GetTitleCnmts(libraryTitle.TitleId);
            libraryTitle = await AggregateLibraryTitle(libraryTitle, titledbTitle, titleDbCnmt);
            await _dataService.AddLibraryTitleAsync(libraryTitle);
            */
            return title;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing file: {file}", file);
            return null;
        }
    }
}