using NsxLibraryManager.Shared.Dto;

namespace NsxLibraryManager.Services.Interface;

public interface IStatsService
{
    public CategoryCountDto[] GetTopCategories(int maxCount = 10);
    public PublisherCountDto[] GetTopPublishers(int maxCount = 10);
    public ContentDistributionCountDto[] GetTopContentDistribution(int maxCount = 10);
    public PackageDistributionCountDto[] GetTopPackageDistribution(int maxCount = 10);

}