using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Data;

public class NsxLibraryDbContext : DbContext
{
    private readonly AppSettings _configuration;
    public DbSet<Title> Titles { get; set; }
    
    public NsxLibraryDbContext(DbContextOptions<NsxLibraryDbContext> options, IOptions<AppSettings> configuration) :
        base(options)
    {
        _configuration = configuration.Value;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=nsxlibrary.db");
        }
    }

}