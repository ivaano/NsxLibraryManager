using System.Collections.ObjectModel;
using System.Linq.Dynamic.Core;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Core.Extensions;
using NsxLibraryManager.Core.Services.Interface;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Models;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;
using NsxLibraryManager.Utils;
using Radzen;
using RatingsContent = NsxLibraryManager.Models.NsxLibrary.RatingsContent;
using Version = NsxLibraryManager.Models.NsxLibrary.Version;


namespace NsxLibraryManager.Services;

public class TitleLibraryService(
    NsxLibraryDbContext nsxLibraryDbContext,
    TitledbDbContext titledbDbContext,
    IFileInfoService fileInfoService,
    IRenamerService renamerService,
    ISettingsService settingsService,
    ILogger<TitleLibraryService> logger)
    : ITitleLibraryService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext =
        nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));

    private readonly TitledbDbContext _titledbDbContext =
        titledbDbContext ?? throw new ArgumentNullException(nameof(titledbDbContext));

    private readonly IRenamerService _renamerService = renamerService;
    private readonly UserSettings _userSettings = settingsService.GetUserSettings();


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
        if (titledbTitle.RatingContents is null || titledbTitle.RatingContents.Count == 0 ||
            titledbTitle.ContentType != TitleContentType.Base) return;
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
        await fileInfoService.DeleteIconDirectoryFiles();
        await _nsxLibraryDbContext.Screenshots.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.Versions.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.Titles.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.TitleCategory.ExecuteDeleteAsync();
        await _nsxLibraryDbContext.RatingsContent.ExecuteDeleteAsync();
        return true;
    }

    public async Task<IEnumerable<string>> GetLibraryFilesAsync()
    {
        var filesResult = await fileInfoService.GetFileNames(_userSettings.LibraryPath, _userSettings.Recursive);
        if (filesResult.IsSuccess) return filesResult.Value;
        logger.LogError("Error getting files from library: {Error}", filesResult.Error);
        return Array.Empty<string>();
    }


    private async Task<Title?> AggregateLibraryTitle(LibraryTitleDto libraryTitle)
    {
        var titledbTitle = await _titledbDbContext
            .Titles
            .Include(t => t.Languages)
            .Include(r => r.RatingContents)
            .Include(c => c.Categories)
            .Include(s => s.Screenshots)
            .Include(v => v.Versions)
            .FirstOrDefaultAsync(t => t.ApplicationId == libraryTitle.ApplicationId);

        var nsxLibraryTitle = new Title
        {
            FileName = libraryTitle.FileName!,
            ApplicationId = libraryTitle.ApplicationId,
        };

        if (titledbTitle is null)
        {
            var metaFromFileName = await fileInfoService.GetFileInfoFromFileName(libraryTitle.FileName!);
            
            nsxLibraryTitle.TitleName = string.IsNullOrEmpty(libraryTitle.TitleName)
                ? metaFromFileName.TitleName
                : libraryTitle.TitleName;

            //If no Type try use the filename
            if (libraryTitle.ContentType == TitleContentType.Unknown)
            {
                nsxLibraryTitle.ContentType = metaFromFileName.ContentType switch
                {
                    TitleContentType.Base => TitleContentType.Base,
                    TitleContentType.Update => TitleContentType.Update,
                    TitleContentType.DLC => TitleContentType.DLC,
                    _ => TitleContentType.Unknown
                };
            }
            else
            {
                nsxLibraryTitle.ContentType = libraryTitle.ContentType switch
                {
                    TitleContentType.Base => TitleContentType.Base,
                    TitleContentType.Update => TitleContentType.Update,
                    TitleContentType.DLC => TitleContentType.DLC,
                    _ => TitleContentType.Unknown
                };
            }

            var iconUrl = libraryTitle.IconUrl;
            if (nsxLibraryTitle.ContentType == TitleContentType.Base)
            { 
                iconUrl = await fileInfoService.GetFileIcon(libraryTitle.FileName!);
            }

            nsxLibraryTitle.BannerUrl = libraryTitle.BannerUrl;
            nsxLibraryTitle.Description = libraryTitle.Description;
            nsxLibraryTitle.Developer = libraryTitle.Developer;
            nsxLibraryTitle.DlcCount = libraryTitle.DlcCount ?? 0;
            nsxLibraryTitle.IconUrl = iconUrl;
            nsxLibraryTitle.Intro = libraryTitle.Intro;
            nsxLibraryTitle.LastWriteTime = libraryTitle.LastWriteTime;
            nsxLibraryTitle.NsuId = libraryTitle.NsuId;
            nsxLibraryTitle.NumberOfPlayers = libraryTitle.NumberOfPlayers;
            nsxLibraryTitle.OwnedDlcs = 0;
            nsxLibraryTitle.OwnedUpdates = 0;
            nsxLibraryTitle.PackageType = metaFromFileName.PackageType;
            nsxLibraryTitle.Publisher = libraryTitle.Publisher;
            nsxLibraryTitle.Rating = libraryTitle.Rating;
            nsxLibraryTitle.ReleaseDate = libraryTitle.ReleaseDate;
            nsxLibraryTitle.Size = metaFromFileName.Size;
            nsxLibraryTitle.Version = libraryTitle.Version;
            return nsxLibraryTitle;
        }

        await AddTitleLanguages(nsxLibraryTitle, titledbTitle);
        await AddTitleCategories(nsxLibraryTitle, titledbTitle);
        await AddRatingContent(nsxLibraryTitle, titledbTitle);

        nsxLibraryTitle.BannerUrl = titledbTitle.BannerUrl;
        nsxLibraryTitle.ContentType = titledbTitle.ContentType;
        nsxLibraryTitle.Description = titledbTitle.Description;
        nsxLibraryTitle.Developer = titledbTitle.Developer;
        nsxLibraryTitle.DlcCount = titledbTitle.DlcCount ?? 0;
        nsxLibraryTitle.IconUrl = titledbTitle.IconUrl;
        nsxLibraryTitle.Intro = titledbTitle.Intro;
        nsxLibraryTitle.LastWriteTime = libraryTitle.LastWriteTime;
        nsxLibraryTitle.LatestVersion = titledbTitle.LatestVersion;
        nsxLibraryTitle.NsuId = titledbTitle.NsuId;
        nsxLibraryTitle.NumberOfPlayers = titledbTitle.NumberOfPlayers;
        nsxLibraryTitle.OtherApplicationId = titledbTitle.OtherApplicationId;
        nsxLibraryTitle.OwnedDlcs = 0;
        nsxLibraryTitle.OwnedUpdates = 0;
        nsxLibraryTitle.PackageType = libraryTitle.PackageType;
        nsxLibraryTitle.Rating = titledbTitle.Rating;
        nsxLibraryTitle.Region = titledbTitle.Region;
        nsxLibraryTitle.ReleaseDate = titledbTitle.ReleaseDate;
        // prefer size from actual file
        nsxLibraryTitle.Size = await fileInfoService.GetFileSize(libraryTitle.FileName!);
        // prefer the title name from titledb
        nsxLibraryTitle.TitleName = libraryTitle.TitleName.ConvertNullOrEmptyTo(titledbTitle.TitleName);
        // prefer the publisher from the file
        nsxLibraryTitle.Publisher = titledbTitle.Publisher.ConvertNullOrEmptyTo(libraryTitle.Publisher);
        
        if (_userSettings.UseEnglishNaming)
        {
            if (LanguageChecker.IsNonEnglish(nsxLibraryTitle.TitleName) || LanguageChecker.IsNonEnglish(nsxLibraryTitle.Publisher))
            {
                var applicationId = titledbTitle.ContentType switch
                {
                    TitleContentType.Base => titledbTitle.ApplicationId,
                    TitleContentType.Update => titledbTitle.OtherApplicationId,
                    TitleContentType.DLC => titledbTitle.OtherApplicationId,
                    _ => titledbTitle.ApplicationId
                };
                    
                var nswName = _titledbDbContext.NswReleaseTitles.FirstOrDefault(x => x.ApplicationId == applicationId);
                if (nswName is not null)
                {
                    if (titledbTitle.ContentType is TitleContentType.DLC)
                    {
                        var nswDlcName = _titledbDbContext.NswReleaseTitles.FirstOrDefault(x => x.ApplicationId == titledbTitle.ApplicationId);
                        if (nswDlcName is not null)
                        {
                            nsxLibraryTitle.TitleName = nswDlcName.TitleName;
                        }
                    }
                    else
                    {
                        nsxLibraryTitle.TitleName = nswName.TitleName;
                    }
                    nsxLibraryTitle.Publisher = nswName.Publisher;
                } 
            }
        }
        nsxLibraryTitle.UpdatesCount = titledbTitle.UpdatesCount;
        nsxLibraryTitle.Version = libraryTitle.Version;

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
        
        if (libraryTitle.ContentType == TitleContentType.DLC)
        {
            nsxLibraryTitle.LatestOwnedUpdateVersion = libraryTitle.Version;
        }

        return nsxLibraryTitle;
    }


    public int GetTitlesCountByContentType(TitleContentType titleContentType)
    {
        return _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Count(t => t.ContentType == titleContentType);
    }

    public async Task<Result<GetBaseTitlesResultDto>> GetDuplicateTitles(TitleContentType contentType)
    {

        var duplicateFileNames = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == contentType)
            .GroupBy(t => t.ApplicationId)
            .Where(g => g.Count() >= 2)
            .Select(g => g.Key)
            .ToListAsync();

        var allDuplicateTitles = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => duplicateFileNames.Contains(t.ApplicationId))
            .OrderBy(t => t.TitleName)
            .ThenBy(t => t.ApplicationId)
            .ThenByDescending(t => t.Version)
            .Select(t => t.MapLibraryTitleDtoNoDlcOrUpdates())
            .ToList();

        var processedTitles = allDuplicateTitles
            .GroupBy(t => t.ApplicationId)
            .SelectMany(group =>
            {
                var titles = group.ToList();
                for (var i = 1; i < titles.Count; i++)
                {
                    titles[i].IsDuplicate = true;
                }
                return titles;
            })
            .ToList();
        
        var count = processedTitles.Count;
        var successResult = new GetBaseTitlesResultDto
        {
            Count = count,
            Titles = processedTitles
        };
        
        return Result.Success(successResult);
    }

    public async Task<Result<bool>> RemoveDuplicateTitles(IList<LibraryTitleDto> titleDtos)
    {
        foreach (var titleDto in titleDtos)
        {
            await RemoveDuplicateTitle(titleDto);
        }

        return Result.Success(true);
    }

    public async Task<Result<bool>> RemoveDuplicateTitle(LibraryTitleDto titleDto)
    {
        var sourceFile = titleDto.FileName;
        if (!string.IsNullOrEmpty(_userSettings.BackupPath) && !string.IsNullOrEmpty(sourceFile))
        {
            var fileName = Path.GetFileName(sourceFile);
            var destinationFile = Path.Combine(_userSettings.BackupPath, fileName);
            if (File.Exists(destinationFile))
            {
                var guid = Guid.NewGuid().ToString("N"); 
                destinationFile = Path.Combine(_userSettings.BackupPath, $"{guid}_{fileName}");
            }
            File.Move(sourceFile, destinationFile);
        }
        await RemoveLibraryTitleAsync(titleDto);
        return Result.Success(true);
    }

    private static async Task<GetBaseTitlesResultDto> ApplyAdditionalFilters(IQueryable<Title> query, LoadDataArgs args)
    {
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        if (args.Filters is not null)
        {
            if (args.Filters.Any())
            {
                var categories = args.Filters
                    .Where(fd => fd.Property == "Category")
                    .Select(x => (string)x.FilterValue)
                    .ToList();
                var otherFilters = args.Filters
                    .Where(fd => fd.Property != "Category")
                    .ToList();

                if (categories.Count != 0)
                {
                    query = query.Where(c => c.Categories.Any(x => categories.Contains(x.Name)));
                }

                if (otherFilters.Count != 0)
                {
                    query = query.Where(FilterBuilder.BuildFilterString(otherFilters));
                }
            }
        }

        if (!string.IsNullOrEmpty(args.OrderBy))
        {
            query = query.OrderBy(args.OrderBy);
        }

        var finalQuery = query.Select(t => t.MapLibraryTitleDtoNoDlcOrUpdates());

        var count = await finalQuery.CountAsync();
        var titles = await finalQuery
            .Skip(args.Skip.Value)
            .Take(args.Top.Value)
            .ToListAsync();

        return new GetBaseTitlesResultDto
        {
            Count = count,
            Titles = titles
        };
    }

    public async Task<Result<GetBaseTitlesResultDto>> GetTitles(LoadDataArgs args)
    {
        var query = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Include(x => x.RatingsContents)
            .Include(x => x.Categories)
            .Include(x => x.Collection)
            .AsQueryable();
        var titles = await ApplyAdditionalFilters(query, args);

        return titles.Count > 0
            ? Result.Success(titles)
            : Result.Failure<GetBaseTitlesResultDto>("No Titles");
    }

    public async Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingLastUpdate(LoadDataArgs args)
    {
        var query = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == TitleContentType.Base)
            .Where(t => t.Versions != null && t.Versions.Count > 0 && t.LatestOwnedUpdateVersion < t.LatestVersion)
            .Include(x => x.Versions.OrderByDescending(v => v.VersionNumber).Take(1))
            .AsQueryable();
        
        return await ApplyAdditionalFilters(query, args);
    }
    
    public async Task<GetBaseTitlesResultDto> GetDlcTitlesWithMissingLastUpdate(LoadDataArgs args)
    {
       
        var query = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == TitleContentType.DLC)
            .Where(t => t.LatestOwnedUpdateVersion < t.LatestVersion)
            .AsQueryable();
        
        return await ApplyAdditionalFilters(query, args);
    }

    public async Task<GetBaseTitlesResultDto> GetBaseTitlesWithMissingDlc(LoadDataArgs args)
    {
        var query = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == TitleContentType.Base)
            .Where(t => t.OwnedDlcs < t.DlcCount)
            .Include(x => x.RatingsContents)
            .Include(x => x.Categories)
            .Include(x => x.Versions.OrderByDescending(v => v.VersionNumber).Take(1))
            .Include(x => x.Screenshots)
            .Include(x => x.Languages).AsQueryable();

        return await ApplyAdditionalFilters(query, args);
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
                            .Where(t => t.OtherApplicationId == title.ApplicationId &&
                                        t.ContentType == TitleContentType.Update)
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

    public async Task<Result<IEnumerable<string>>> GetCategoriesAsync()
    {
        var categories = await _nsxLibraryDbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        return categories.Count > 0
            ? Result.Success<IEnumerable<string>>(categories)
            : Result.Failure<IEnumerable<string>>("No Categories");
    }

    #region Collections

    public async Task<Result<IEnumerable<CollectionDto>>> GetCollections()
    {
        var collections = await _nsxLibraryDbContext.Collections
            .AsNoTracking()
            .Select(x => new
            {
                Collection = x,
                TitlesCount = x.Titles.Count(),
                BaseTitlesCount = x.Titles.Count(t => t.ContentType == TitleContentType.Base),
                DlcTitlesCount = x.Titles.Count(t => t.ContentType == TitleContentType.DLC),
                UpdatesTitlesCount = x.Titles.Count(t => t.ContentType == TitleContentType.Update)

            })
            .OrderBy(c => c.Collection.Name)
            .Select(x => x.Collection.MapToCollectionDto(x.TitlesCount, x.BaseTitlesCount, x.DlcTitlesCount, x.UpdatesTitlesCount))
            .ToListAsync();

        return collections.Count > 0
            ? Result.Success<IEnumerable<CollectionDto>>(collections)
            : Result.Failure<IEnumerable<CollectionDto>>("No Collections");
    }

    public async Task<Result<CollectionDto?>> AddCollection(CollectionDto collectionDto)
    {
        var exists = _nsxLibraryDbContext.Collections
            .AsNoTracking()
            .Any(c => c.Name == collectionDto.Name);
        if (exists) return Result.Failure<CollectionDto?>("Collection already exists");
        if (collectionDto.Name is null) return Result.Failure<CollectionDto?>("Collection name is required");

        var collection = new Collection
        {
            Name = collectionDto.Name,
        };
        _nsxLibraryDbContext.Collections.Add(collection);
        await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(collection.MapToCollectionDto());
    }

    public async Task<Result<CollectionDto?>> RemoveCollection(CollectionDto collectionDto)
    {
        var collection = await _nsxLibraryDbContext.Collections.FirstOrDefaultAsync(c => c.Id == collectionDto.Id);
        if (collection is null) return Result.Failure<CollectionDto?>("Collection already exists");
        var deleteResult = _nsxLibraryDbContext.Collections.Remove(collection);
        await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(deleteResult.Entity.MapToCollectionDto());
    }

    public async Task<Result<CollectionDto?>> UpdateCollection(CollectionDto collectionDto)
    {
        var collection = await _nsxLibraryDbContext.Collections.FirstOrDefaultAsync(c => c.Id == collectionDto.Id);
        if (collection is null) return Result.Failure<CollectionDto?>("Collection already exists");
        if (collectionDto.Name is null) return Result.Failure<CollectionDto?>("Collection name is required");
        collection.Name = collectionDto.Name;
        await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(collection.MapToCollectionDto());
    }

    #endregion

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
            LibraryPath = _userSettings.LibraryPath
        };
        _nsxLibraryDbContext.LibraryUpdates.Add(reloadRecord);
        await _nsxLibraryDbContext.SaveChangesAsync();
    }

    public async Task<FileDelta> GetDeltaFilesInLibraryAsync()
    {
        var libraryFiles = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .ToDictionaryAsync(x => x.FileName, x => x);

        var libDirFiles = await GetLibraryFilesAsync();

        var dirFiles = new Dictionary<string, LibraryTitleDto>();
        foreach (var fileName in libDirFiles)
        {
            if (fileInfoService.TryGetFileInfoFromFileName(fileName, out var fileInfo))
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
        var toRemove = filesToRemove.Select(x => new LibraryTitleDto
        {
            ApplicationId = libraryFiles[x].ApplicationId,
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

    public async Task<Result<LibraryTitleDto>> GetTitleByApplicationId(string applicationId)
    {
        var title = await _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Include(x => x.Collection)
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

        return title is not null
            ? Result.Success(title.MapToLibraryTitleDto(relatedTitles, relatedTitlesTitleDb))
            : Result.Failure<LibraryTitleDto>("Title not found");
    }

    public async Task<Result<bool>> AddLibraryTitleAsync(LibraryTitleDto title)
    {
        if (title.FileName is null) return Result.Failure<bool>("Filename missing");
        logger.LogDebug("Adding file: {Filename} from library", title.FileName);
        var libraryTitle = await ProcessFileAsync(title.FileName);

        switch (libraryTitle)
        {
            case null:
                return Result.Failure<bool>($"Error processing file {title.FileName}");
            case { ContentType: TitleContentType.Update, OtherApplicationId: not null }:
            {
                var parentTitle = _nsxLibraryDbContext.Titles
                    .Include(x => x.Collection)
                    .FirstOrDefault(x => x.ApplicationId == libraryTitle.OtherApplicationId);

                if (parentTitle?.Collection is not null)
                {
                    libraryTitle.Collection = parentTitle.Collection;
                }

                var updateCount = _nsxLibraryDbContext.Titles
                    .Count(x => x.ContentType == TitleContentType.Update &&
                                x.OtherApplicationId == libraryTitle.OtherApplicationId);
                if (parentTitle is not null) parentTitle.OwnedUpdates = updateCount;
                break;
            }

            case { ContentType: TitleContentType.DLC, OtherApplicationId: not null }:
            {
                var parentTitle = _nsxLibraryDbContext.Titles
                    .Include(x => x.Collection)
                    .FirstOrDefault(x => x.ApplicationId == libraryTitle.OtherApplicationId);

                if (parentTitle?.Collection is not null)
                {
                    libraryTitle.Collection = parentTitle.Collection;
                }

                var dlcCount = _nsxLibraryDbContext.Titles
                    .Count(x => x.ContentType == TitleContentType.DLC &&
                                x.OtherApplicationId == libraryTitle.OtherApplicationId);
                if (parentTitle is not null) parentTitle.OwnedDlcs = dlcCount;

                break;
            }
        }

        await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(true);
    }

    public async Task<Result<int>> UpdateMultipleLibraryTitlesAsync(IEnumerable<LibraryTitleDto> titles)
    {
        var updatedCount = 0;
        foreach (var title in titles)
        {
            var libraryTitle = await _nsxLibraryDbContext.Titles.FirstOrDefaultAsync(x => x.Id == title.Id);
            if (libraryTitle is null) return Result.Failure<int>("Title not found");
            updatedCount += await UpdateCollectionAssociationsAsync(libraryTitle, title);
        }

        await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(updatedCount);
    }

    public async Task<Result<int>> UpdateLibraryTitleAsync(LibraryTitleDto title)
    {
        //update by filename only
        if (title.Id == 0)
        {
            var removeResult = await RemoveLibraryTitleAsync(title);
            if (removeResult.IsFailure) return Result.Failure<int>(removeResult.Error!);

            var addTitleResult = await AddLibraryTitleAsync(title);
            if (addTitleResult.IsFailure) return Result.Failure<int>(removeResult.Error!);
            return Result.Success(1);
        }

        var libraryTitle = await _nsxLibraryDbContext.Titles.FirstOrDefaultAsync(x => x.Id == title.Id);
        if (libraryTitle is null) return Result.Failure<int>("Title not found");

        var updatedCollectionCount = await UpdateCollectionAssociationsAsync(libraryTitle, title);
        libraryTitle.UserRating = title.UserRating;
        var updatedCount = await _nsxLibraryDbContext.SaveChangesAsync();
        return Result.Success(updatedCount);
    }

    private async Task<int> UpdateCollectionAssociationsAsync(Title libraryTitle, LibraryTitleDto titleDto)
    {
        if (titleDto.Collection is not null)
        {
            return await AssignCollectionAsync(libraryTitle, titleDto);
        }

        return await RemoveFromCollectionAsync(libraryTitle);
    }

    private async Task<int> AssignCollectionAsync(Title libraryTitle, LibraryTitleDto titleDto)
    {
        var collection = await _nsxLibraryDbContext.Collections
            .FirstOrDefaultAsync(x => titleDto.Collection != null && x.Id == titleDto.Collection.Id);

        var titlesToUpdate = await GetRelatedTitlesAsync(libraryTitle, titleDto.ContentType);
        var toUpdate = titlesToUpdate.ToList();
        foreach (var title in toUpdate)
        {
            title.Collection = collection;
        }

        return toUpdate.Count;
    }

    private async Task<int> RemoveFromCollectionAsync(Title libraryTitle)
    {
        var titlesToUpdate = await GetRelatedTitlesAsync(libraryTitle, libraryTitle.ContentType);
        var updateCount = 0;

        foreach (var title in titlesToUpdate)
        {
            var collection = await _nsxLibraryDbContext.Collections
                .FirstOrDefaultAsync(x => x.Titles.Contains(title));
            if (collection?.Titles.Remove(title) == true)
            {
                updateCount++;
            }
        }

        return updateCount;
    }

    private async Task<IEnumerable<Title>> GetRelatedTitlesAsync(Title libraryTitle, TitleContentType contentType)
    {
        var titles = new List<Title>();

        if (contentType == TitleContentType.Base)
        {
            titles.AddRange(await _nsxLibraryDbContext.Titles
                .Where(x => x.OtherApplicationId == libraryTitle.ApplicationId)
                .ToListAsync());
            titles.Add(libraryTitle);
        }
        else
        {
            var parentTitle = await _nsxLibraryDbContext.Titles
                .FirstOrDefaultAsync(x => x.ApplicationId == libraryTitle.OtherApplicationId);

            if (parentTitle != null)
            {
                titles.Add(parentTitle);
                titles.AddRange(await _nsxLibraryDbContext.Titles
                    .Where(x => x.OtherApplicationId == libraryTitle.OtherApplicationId)
                    .ToListAsync());
            }
        }

        return titles;
    }

    public async Task<Result<bool>> RemoveLibraryTitleAsync(LibraryTitleDto title)
    {
        logger.LogDebug("Removing file: {Filename} from library", title.FileName);
        var libraryTitle =
            _nsxLibraryDbContext.Titles.FirstOrDefault(x =>
                x.ApplicationId == title.ApplicationId && x.FileName == title.FileName);

        switch (libraryTitle)
        {
            case null:
                return Result.Failure<bool>($"Title {title.ApplicationId} with filename {title.FileName} not found");
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
            logger.LogError(ex, "Error removing file: {Filename} from library", title.FileName);
        }

        return Result.Success(true);
    }

    public async Task<Title?> ProcessFileAsync(string file)
    {
        try
        {
            logger.LogDebug("Processing file: {File}", file);
            var libraryTitleResult = await fileInfoService.GetFileInfo(file, detailed: false);
            if (libraryTitleResult.IsFailure)
            {
                logger.LogError("Unable to get File Information from file : {File}", file);
                return null;
            }

            var title = await AggregateLibraryTitle(libraryTitleResult.Value);
            if (title is null) return title;
            _nsxLibraryDbContext.Add(title);
            await _nsxLibraryDbContext.SaveChangesAsync();
            return title;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error processing file: {File}", file);
            return null;
        }
    }

    public async Task<Result<IEnumerable<RenameTitleDto>>> GetLibraryFilesToRenameAsync(
        RenameType renameType)
    {
        var fileList = new List<RenameTitleDto>();
        if (renameType == RenameType.Collection)
        {
            var collectionFiles = await _nsxLibraryDbContext.Titles
                .AsNoTracking()
                .Include(t => t.Collection)
                .Where(t => t.Collection != null)
                .Select(t => t.MapLibraryTitleDtoNoDlcOrUpdates())
                .ToListAsync();

            foreach (var libraryTitle in collectionFiles)
            {
                var fileTemplate =
                    _renamerService.GetRenameTemplate(RenameType.Collection, libraryTitle.ContentType, libraryTitle.PackageType);
                if (!fileTemplate.IsSuccess) continue;
                if (libraryTitle.ContentType is TitleContentType.Update or TitleContentType.DLC &&
                    string.IsNullOrEmpty(libraryTitle.OtherApplicationName))
                {
                    var otherTitle = await _nsxLibraryDbContext.Titles
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.ApplicationId == libraryTitle.OtherApplicationId);
                    if (otherTitle is not null) libraryTitle.OtherApplicationName = otherTitle.TitleName;
                }
                var newFileName = await _renamerService.GetNewFileName(fileTemplate.Value, libraryTitle, RenameType.Collection);
                if (newFileName.IsFailure)
                {
                    fileList.Add(new RenameTitleDto
                    {
                        SourceFileName = libraryTitle.FileName ?? string.Empty,
                        DestinationFileName = string.Empty,
                        TitleId = libraryTitle.ApplicationId,
                        TitleName = libraryTitle.TitleName,
                        Error = true,
                        ErrorMessage = newFileName.Error,
                    });  
                    continue;
                }

                if (newFileName.Value != libraryTitle.FileName)
                {
                    fileList.Add(new RenameTitleDto
                    {
                        SourceFileName = libraryTitle.FileName ?? string.Empty,
                        DestinationFileName = newFileName.Value,
                        TitleId = libraryTitle.ApplicationId,
                        TitleName = libraryTitle.TitleName,
                        Error = false,
                        ErrorMessage = string.Empty,
                        Id = libraryTitle.Id,
                        UpdateLibrary = true
                    });
                }
            }
        }


        return Result.Success(fileList.AsEnumerable());
    }
}