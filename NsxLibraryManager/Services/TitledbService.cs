using System.Globalization;
using System.IO.Compression;
using System.Linq.Dynamic.Core;
using Common.Services;
using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Data;
using NsxLibraryManager.Extensions;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.ViewModels.TitleDb;
using Radzen;
using FileInfo = System.IO.FileInfo;

namespace NsxLibraryManager.Services;

public class TitledbService : ITitledbService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<TitledbService> _logger;
    private readonly IConfiguration _configuration;
    private readonly TitledbDbContext _titledbDbContext;


    public TitledbService(
        ILogger<TitledbService> logger,
        IConfiguration configuration,
        TitledbDbContext titledbDbContext,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceScopeFactory = serviceScopeFactory;
        _titledbDbContext = titledbDbContext;
    }

    private void DecompressFile(string compressedFilePath, string decompressedFilePath)
    {
        using var compressedFileStream = new FileStream(compressedFilePath, FileMode.Open, FileAccess.Read);
        using var decompressedFileStream = new FileStream(decompressedFilePath, FileMode.Create, FileAccess.Write);
        using var gzipStream = new GZipStream(compressedFileStream, CompressionMode.Decompress);
        gzipStream.CopyTo(decompressedFileStream);
    }

    public async Task<LibraryTitleDto?> GetTitleByApplicationId(string applicationId)
    {
        var title = await _titledbDbContext.Titles
            .AsNoTracking()
            .Include(l => l.Languages)
            .Include(v => v.Versions)
            .Include(c => c.Categories)
            .Include(s => s.Screenshots)
            .FirstOrDefaultAsync(t => t.ApplicationId == applicationId);

        var relatedTitles = await _titledbDbContext.Titles
            .AsNoTracking()
            .Where(t => t.OtherApplicationId == applicationId)
            .OrderByDescending(t => t.ReleaseDate)
            .ToListAsync();


        return title?.MapToTitleDtoWithOtherTitles(relatedTitles);
    }

    public Result<DbHistoryDto> GetLatestTitledbVersionAsync()
    {
        var dbHistory = _titledbDbContext.History.OrderByDescending(h => h.TimeStamp).FirstOrDefault();
        if (dbHistory is null) return Result.Failure<DbHistoryDto>("No History");
        
        var result = new DbHistoryDto
        {
            Version = dbHistory.VersionNumber,
            Date = dbHistory.TimeStamp.ToString(CultureInfo.CurrentCulture)
        };
        return Result.Success(result);
    }

    public async Task<Result<IEnumerable<string>>> GetCategoriesAsync()
    {
        var categories = await _titledbDbContext.Categories
            .OrderBy(c => c.Name)
            .Select(c =>  c.Name )
            .AsNoTracking()
            .ToListAsync();
        
        return categories.Count > 0 
            ? Result.Success<IEnumerable<string>>(categories) 
            : Result.Failure<IEnumerable<string>>("No Categories");
    }

    public async Task<Result<GridPageViewModel>> GetTitles(LoadDataArgs args, IEnumerable<string>? selecteCategories)
    {
        var query = _titledbDbContext.Titles
            .AsNoTracking()
            .Include(x => x.RatingContents)
            .Include(x => x.Categories)
            .Include(x => x.Versions)
            .Include(x => x.Screenshots)
            .Include(x => x.Languages)
            .AsQueryable();

        if (selecteCategories is not null)
        {
            var categories = selecteCategories as string[] ?? selecteCategories.ToArray();
            if (categories.Length != 0)
            {
                query = query
                    .Where(c => c.Categories!.Any(x => categories.Contains(x.Name)));
            }
        }

        
        if (!string.IsNullOrEmpty(args.Filter))
        {
            query = query.Where(args.Filter);
        }

        query = !string.IsNullOrEmpty(args.OrderBy) ? 
            query.OrderBy(args.OrderBy) : 
            query.OrderBy(x => x.TitleName);
        
        var count = await query.CountAsync();
        
        var titles = await query
            .Select(t => t.MapToTitleDto())
            .Skip(args.Skip!.Value)
            .Take(args.Top!.Value)
            .ToListAsync();

        if (count > 0)
        {
            return Result.Success(new GridPageViewModel
            {
                TotalRecords = count,
                Titles = titles
            });            
        }
        return Result.Failure<GridPageViewModel>("No Titles");
    }

    public Task ReplaceDatabase(string compressedFilePath, CancellationToken cancellationToken = default)
    {
        try
        {
            var databasePath = _configuration.GetSection("NsxLibraryManager:TitledbDBConnection").Value.CleanDatabasePath();

            if (!string.IsNullOrEmpty(databasePath))
            {
                
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    // Important: Do NOT retrieve the context here yet!
                    var destPath = compressedFilePath.Remove(compressedFilePath.IndexOf(".gz", StringComparison.Ordinal));
                    DecompressFile(compressedFilePath, destPath);
                    File.Copy(destPath, databasePath, true);
                    var fileInfo = new FileInfo(databasePath);

                    if (fileInfo.Length != new FileInfo(databasePath).Length)
                    {
                        throw new Exception("New titldb copy seems incomplete. Please retry.");
                    }
                
                    using (var newContext = scope.ServiceProvider.GetRequiredService<TitledbDbContext>())
                    {
                        // Verify the new database
                        var count = newContext.Titles.Count();
                        _logger.LogInformation("Number of titles in new db: {}", count);
                    }
                }
            }
            else
            {
                _logger.LogError("Unable to get titledb db connection from config.json.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error replacing database: {}", ex.Message);
            throw;
        }

        return Task.CompletedTask;
    }
}