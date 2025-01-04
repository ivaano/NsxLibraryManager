namespace NsxLibraryManager.Providers;

public class LiveDatabaseConfigurationSource : IConfigurationSource
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LiveDatabaseConfigurationSource(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new LiveDatabaseConfigurationProvider(_scopeFactory);
    }
}