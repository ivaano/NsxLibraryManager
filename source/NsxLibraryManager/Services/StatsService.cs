using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Data;
using NsxLibraryManager.Services.Interface;
using NsxLibraryManager.Shared.Dto;
using NsxLibraryManager.Shared.Enums;
using NsxLibraryManager.Shared.Settings;
using Radzen;

namespace NsxLibraryManager.Services;

public class StatsService : IStatsService
{
    private readonly NsxLibraryDbContext _nsxLibraryDbContext;
    private readonly UserSettings _configuration;
    private readonly ILogger<StatsService> _logger;
    
    public StatsService(
        NsxLibraryDbContext nsxLibraryDbContext,
        ISettingsService settingsService,
        ILogger<StatsService> logger)
    {
        _nsxLibraryDbContext = nsxLibraryDbContext ?? throw new ArgumentNullException(nameof(nsxLibraryDbContext));
        _configuration = settingsService.GetUserSettings();
        _logger = logger;
    }

    public CategoryCountDto[] GetTopCategories(int maxCount = 10)
    {
        var categoryCounts = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Include(t => t.Categories)
            .Where(t => t.ContentType == TitleContentType.Base)
            .SelectMany(t => t.Categories)
            .GroupBy(t => t.Id)
            .Select(g => new CategoryCountDto
            {
                Category = g.First().Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count) 
            .Take(maxCount)
            .ToArray();

        return categoryCounts;
    }

    public PublisherCountDto[] GetTopPublishers(int maxCount = 10)
    {
        var publisherCounts = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .Where(t => t.ContentType == TitleContentType.Base)
            .GroupBy(t => t.Publisher)
            .Select(g => new PublisherCountDto
            {
                Publisher = g.Key ?? string.Empty,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count) 
            .Take(maxCount)
            .ToArray();
        
        return publisherCounts;
    }

    public ContentDistributionCountDto[] GetTopContentDistribution(int maxCount = 10)
    {
        var contentDistributionCount = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .GroupBy(t => t.ContentType)
            .Select(g => new ContentDistributionCountDto
            {
                Type = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count) 
            .Take(maxCount)
            .ToArray();
        
        return contentDistributionCount;
    }

    public PackageDistributionCountDto[] GetTopPackageDistribution(int maxCount = 10)
    {
        var packageDistribution = _nsxLibraryDbContext.Titles
            .AsNoTracking()
            .GroupBy(t => t.PackageType)
            .Select(g => new PackageDistributionCountDto
            {
                Type = g.Key.ToString(),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count) 
            .Take(maxCount)
            .ToArray();
        
        return packageDistribution;
    }
}