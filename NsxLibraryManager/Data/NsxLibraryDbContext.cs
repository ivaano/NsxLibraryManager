using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Models.NsxLibrary;
using Version = NsxLibraryManager.Models.NsxLibrary.Version;

namespace NsxLibraryManager.Data;

public class NsxLibraryDbContext : DbContext
{
    private readonly UserSettings _configuration;
    public DbSet<Title> Titles { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<TitleCategory> TitleCategory { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }
    public DbSet<Version> Versions { get; set; }

    public DbSet<CategoryLanguage> CategoryLanguages { get; set; }
    public DbSet<Settings> Settings { get; set; }


    public NsxLibraryDbContext(DbContextOptions<NsxLibraryDbContext> options, IOptions<UserSettings> configuration) :
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasMany(e => e.Languages)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId);
        
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Categories)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleCategory>(
                j => j
                    .HasOne(tc => tc.Category)
                    .WithMany(c => c.TitleCategories)
                    .HasForeignKey(tc => tc.CategoryId),
                j => j
                    .HasOne(tc => tc.Title)
                    .WithMany(t => t.TitleCategories)
                    .HasForeignKey(tc => tc.TitleId)
            );
        
    }
}