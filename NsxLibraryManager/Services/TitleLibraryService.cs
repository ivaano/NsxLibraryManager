using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Migrations;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.Dto;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Utils;
using Radzen;
using RatingsContent = NsxLibraryManager.Models.NsxLibrary.RatingsContent;
using Version = NsxLibraryManager.Models.NsxLibrary.Version;


namespace NsxLibraryManager.Services;

public class TitleLibraryService : ITitleLibraryService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext;
    private readonly TitledbDbContext _titledbDbContext;
    private readonly IFileInfoService _fileInfoService;
    private readonly UserSettings _settings;
    private readonly ILogger<TitleLibraryService> _logger;


    public TitleLibraryService(
        NsxLibraryDbContext nsxLibraryDbContext,
        TitledbDbContext titledbDbContext,
        IFileInfoService fileInfoService,
        ISettingsService settingsService,
        ILogger<TitleLibraryService> logger)
    {
        _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
        _titledbDbContext = titledbDbContext ?? throw new ArgumentNullException(nameof(titledbDbContext));
        _fileInfoService = fileInfoService;
        _settings = settingsService.GetUserSettings();
        _logger = logger;
    }

    private static Task<Title> AddTitleLanguages(Title nsxLibraryTitle, Models.Titledb.Title titledbTitle)
    {
        if (titledbTitle.Languages is not { Count: > 0 }) return Task.FromResult(nsxLibraryTitle);
        var libraryLanguages = nsxLibraryTitle.Languages.ToDictionary(x => x.LanguageCode, x => x);
            
        foreach (var language in titledbTitle.Languages)
        {
            if (libraryLanguages.TryGetValue(language.LanguageCode, out var libraryLanguage))
            {
                nsxLibraryTitle.Languages.Add(libraryLanguage);
            }
            else
            {
                var newLibraryLanguage = new Language
                {
                    LanguageCode = language.LanguageCode
                };
                nsxLibraryTitle.Languages.Add(newLibraryLanguage);
            }
        }
        return Task.FromResult(nsxLibraryTitle);
    }
    
    private async Task<Title> AddTitleCategories(Title nsxLibraryTitle, Models.Titledb.Title titledbTitle)
    {
        if (titledbTitle.Categories is null || titledbTitle.Categories.Count == 0) return nsxLibraryTitle;
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
    
    
    private async Task AddRatingContent(Title nsxLibraryTitle, Models.Titledb.Title titledbTitle)
    {
        if (titledbTitle.RatingContents is null || titledbTitle.RatingContents.Count == 0 || titledbTitle.ContentType != TitleContentType.Base ) return;
        var libraryRatings = _nsxLibraryDbContext.RatingsContent.ToDictionary(x => x.Name, x => x);
            
        foreach (var rating in titledbTitle.RatingContents)
        {
            if (libraryRatings.TryGetValue(rating.Name, out var libraryRatingContent))
            {
                nsxLibraryTitle.RatingsContents.Add(libraryRatingContent);
            }
            else
            {
                var newlibraryRatingContent = new RatingsContent
                {
                    Name = rating.Name
                };
                nsxLibraryTitle.RatingsContents.Add(newlibraryRatingContent);
            }
        }

        await Task.FromResult(nsxLibraryTitle);
    }

    
    public async Task<bool> DropLibrary()
    {
        await _nsxLibraryDbContext.Screenshots.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.Versions.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.Titles.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.TitleCategory.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.RatingsContent.ExecuteDeleteAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetFilesAsync()
    {
        var files = await _fileInfoService.GetFileNames(_settings.LibraryPath,
            _settings.Recursive);
        return files;
    }



    private async Task<Title?> AggregateLibraryTitle(LibraryTitle libraryTitle)
    {
        var titledbTitle = await _titledbDbContext
            .Titles
            .Include(t => t.Languages)
            .Include(r => r.RatingContents)
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
                ? metaFromFileName.TitleName
                : libraryTitle.TitleName;

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

        await AddTitleLanguages(nsxLibraryTitle, titledbTitle);
        await AddTitleCategories(nsxLibraryTitle, titledbTitle);
        await AddRatingContent(nsxLibraryTitle, titledbTitle);



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


    public int GetTitlesCountByContentType(TitleContentType titleContentType)
    {
        return _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Count(t => t.ContentType == titleContentType);
    }

    public async Task<GetBaseTitlesResultDto> GetBaseTitles(LoadDataArgs args)
    {
        var query = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == TitleContentType.Base)
            .Include(x => x.RatingsContents)
            .Include(x => x.Categories)
            .Include(x => x.Versions)
            .Include(x => x.Screenshots)
            .Include(x => x.Languages).AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        if (args.Filters is not null)
        {
            if (args.Filters.Any())
            {
                query = query.Where(FilterBuilder.BuildFilterString(args.Filters));
            }
        }

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }
        
        var finalQuery = query.Select(t => t.MapLibraryTitleDtoNoDlcOrUpdates());
        
        return new GetBaseTitlesResultDto
        {
            Count = await finalQuery.CountAsync(),
            Titles = finalQuery
                .Skip(args.Skip.Value)
                .Take(args.Top.Value)
                .ToList()
        };
    }

    public async Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingLastUpdate(LoadDataArgs args)
    {
        var query = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == TitleContentType.Base)
            .Where(t => t.Versions != null && t.Versions.Count > 0 && t.LatestOwnedUpdateVersion < t.LatestVersion)
            .Include(x => x.RatingsContents)
            .Include(x => x.Categories)
            .Include(x => x.Versions!.OrderByDescending(v => v.VersionNumber).Take(1))
            .Include(x => x.Screenshots)
            .Include(x => x.Languages).AsQueryable();
        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        if (args.Filters is not null)
        {
            if (args.Filters.Any())
            {
                query = query.Where(FilterBuilder.BuildFilterString(args.Filters));
            }
        }

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }
        
        var finalQuery = query.Select(t => t.MapLibraryTitleDtoNoDlcOrUpdates());
        
        return new GetBaseTitlesResultDto
        {
            Count = await finalQuery.CountAsync(),
            Titles = finalQuery
                .Skip(args.Skip.Value)
                .Take(args.Top.Value)
                .ToList()
        };
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
                        title.LatestOwnedUpdateVersion = _nsxLibraryDbContext.Titles
                            .AsNoTracking()
                            .Where(t => t.OtherApplicationId == title.ApplicationId && t.ContentType == TitleContentType.Update)
                            .Max(t => t.Version);
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

    public Task<IQueryable<DlcDto>> GetTitleDlcsAsQueryable(string applicationId)
    {
        var libraryApplicationIds = _nsxLibraryDbContext.Titles.AsNoTracking()
            .Where(t => t.OtherApplicationId == applicationId)
            .Where(t => t.ContentType == TitleContentType.DLC)
            .OrderByDescending(t => t.ReleaseDate)
            .Select(t => t.ApplicationId).ToHashSet();

        var queryableDlcs = _titledbDbContext.Titles.AsNoTracking()
            .Where(t => t.OtherApplicationId == applicationId)
            .Where(t => t.ContentType == TitleContentType.DLC)
            .OrderByDescending(t => t.ReleaseDate)
            .Select(t => new DlcDto
            {
                ApplicationId = t.ApplicationId,
                OtherApplicationId = t.OtherApplicationId,
                TitleName = t.TitleName,
                Size = t.Size.GetValueOrDefault(),
                Owned = libraryApplicationIds.Contains(t.ApplicationId)
            }).AsQueryable();

        return Task.FromResult(queryableDlcs);
    }

    public Task<LibraryUpdate?> GetLastLibraryUpdateAsync()
    {
        return Task.FromResult(_nsxLibraryDbContext.LibraryUpdates
            .AsNoTracking()
            .OrderBy(t => t.DateUpdated)
            .LastOrDefault());
    }

    public async Task SaveLibraryReloadDate(bool refresh = false)
    {
        var dateCreated = DateTime.Now;
        if (refresh)
        {
            var previousDate = await GetLastLibraryUpdateAsync();
            dateCreated = previousDate?.DateCreated ?? DateTime.Now;
        }
        
        var reloadRecord = new LibraryUpdate
        {
            DateCreated = dateCreated,
            DateUpdated = DateTime.Now,
            BaseTitleCount = GetTitlesCountByContentType(TitleContentType.Base),
            UpdateTitleCount = GetTitlesCountByContentType(TitleContentType.Update),
            DlcTitleCount = GetTitlesCountByContentType(TitleContentType.DLC),
            LibraryPath = _settings.LibraryPath
        };
        _nsxLibraryDbContext.LibraryUpdates.Add(reloadRecord);
        await _nsxLibraryDbContext.SaveChangesAsync();
    }

    public async Task<FileDelta> GetDeltaFilesInLibraryAsync()
    {
        var libraryFiles = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.FileName, x => x);

        var libDirFiles = await GetFilesAsync();

        var dirFiles = new Dictionary<string, LibraryTitle>();
        foreach (var fileName in libDirFiles)
        {
            if (_fileInfoService.TryGetFileInfoFromFileName(fileName, out var fileInfo))
            {
                dirFiles.Add(fileName, fileInfo);
            }
        }

        var filesToAdd = dirFiles.Keys.Except(libraryFiles.Keys);
        var filesToRemove = libraryFiles.Keys.Except(dirFiles.Keys);


        var filesToUpdate = libraryFiles.Keys
            .Intersect(dirFiles.Keys)
            .Where(fileName =>
                Math.Abs((dirFiles[fileName].LastWriteTime ?? DateTime.MinValue)
                    .Subtract(libraryFiles[fileName].LastWriteTime ?? DateTime.MinValue)
                    .TotalSeconds) > 1)
            .ToList();


        var toAdd = filesToAdd.Select(key => dirFiles[key]).ToList();
        var toRemove = filesToRemove.Select(x => new LibraryTitle
        {
            TitleId = libraryFiles[x].ApplicationId,
            FileName = libraryFiles[x].FileName
        }).ToList();
        var toUpdate = filesToUpdate.Select(key => dirFiles[key]).ToList();
        return new FileDelta
        {
            FilesToAdd = toAdd,
            FilesToRemove = toRemove,
            FilesToUpdate = toUpdate,
            TotalFiles = toAdd.Count + toRemove.Count + filesToUpdate.Count
        };
    }

    public async Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId)
    {
        var title = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Include(l => l.Languages)
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

    public async Task<bool> AddLibraryTitleAsync(LibraryTitle title)
    {
        _logger.LogDebug("Adding file: {filename} from library", title.FileName);
        var libraryTitle = await ProcessFileAsync(title.FileName);

        switch (libraryTitle)
        {
            case null:
                return false;
            case { ContentType: TitleContentType.Update, OtherApplicationId: not null }:
            {
                var parentTitle = _nsxLibraryDbContext.Titles.FirstOrDefault(x => x.ApplicationId == libraryTitle.OtherApplicationId);
                var updateCount = _nsxLibraryDbContext.Titles
                    .Count(x => x.ContentType == TitleContentType.Update && x.OtherApplicationId == libraryTitle.OtherApplicationId);
                if (parentTitle is not null) parentTitle.OwnedUpdates = updateCount;
                break;
            }

            case { ContentType: TitleContentType.DLC, OtherApplicationId: not null }:
            {
                var parentTitle = _nsxLibraryDbContext.Titles.FirstOrDefault(x => x.ApplicationId == libraryTitle.OtherApplicationId);
                var dlcCount = _nsxLibraryDbContext.Titles
                    .Count(x => x.ContentType == TitleContentType.DLC && x.OtherApplicationId == libraryTitle.OtherApplicationId);
                if (parentTitle is not null) parentTitle.OwnedDlcs = dlcCount;

                break;
            }
        }
        await _nsxLibraryDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateLibraryTitleAsync(LibraryTitle title)
    {
        _logger.LogDebug("Updating file: {filename} from library", title.FileName);
        await RemoveLibraryTitleAsync(title);
        await AddLibraryTitleAsync(title);
        return true;
    }
    
    public async Task<bool> RemoveLibraryTitleAsync(LibraryTitle title)
    {
        _logger.LogDebug("Removing file: {filename} from library", title.FileName);
        var libraryTitle =
            _nsxLibraryDbContext.Titles.FirstOrDefault(x =>
                x.ApplicationId == title.TitleId && x.FileName == title.FileName);

        switch (libraryTitle)
        {
            case null:
                return false;
            case { ContentType: TitleContentType.Update, OtherApplicationId: not null }:
            {
                var updateCount =
                    _nsxLibraryDbContext.Titles.FirstOrDefault(x => x.ApplicationId == libraryTitle.OtherApplicationId);
                if (updateCount is not null) updateCount.OwnedUpdates -= 1;
                break;
            }

            case { ContentType: TitleContentType.DLC, OtherApplicationId: not null }:
            {
                var dlcCount =
                    _nsxLibraryDbContext.Titles.FirstOrDefault(x => x.ApplicationId == libraryTitle.OtherApplicationId);
                if (dlcCount is not null) dlcCount.OwnedDlcs -= 1;
                break;
            }
        }

        try
        {
            var titleScreenshots = _nsxLibraryDbContext.Screenshots.Where(t => t.Title == libraryTitle);
            _nsxLibraryDbContext.Screenshots.RemoveRange(titleScreenshots);
            _nsxLibraryDbContext.Titles.Remove(libraryTitle);
            await _nsxLibraryDbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing file: {filename} from library", title.FileName);
        }


        return true;
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
            if (title is null) return title;
            _nsxLibraryDbContext.Add(title);
            await _nsxLibraryDbContext.SaveChangesAsync();
            return title;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing file: {file}", file);
            return null;
        }
    }
}