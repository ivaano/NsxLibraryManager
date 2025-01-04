namespace NsxLibraryManager.Providers;

public class DatabaseConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;

    public DatabaseConfigurationSource(IConfiguration configuration, IServiceScopeFactory scopeFactory)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
    }

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new DatabaseConfigurationProvider(_configuration, _scopeFactory);
    }    
}