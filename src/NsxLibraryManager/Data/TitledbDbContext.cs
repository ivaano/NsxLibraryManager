using Microsoft.EntityFrameworkCore;
using NsxLibraryManager.Models.Titledb;
using Version = NsxLibraryManager.Models.Titledb.Version;


namespace NsxLibraryManager.Data;

public class TitledbDbContext : DbContext
{
    public DbSet<Title> Titles { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }
    public DbSet<Cnmt> Cnmts { get; set; }
    public DbSet<Edition> Editions { get; set; }
    public DbSet<Version> Versions { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<CategoryLanguage> CategoryLanguages { get; set; }
    public DbSet<TitleLanguage> TitleLanguages { get; set; }

    public DbSet<RatingContent> RatingContents { get; set; }
    public DbSet<TitleRatingContent> TitleRatingContents { get; set; }
    public DbSet<History> History { get; set; }
    public DbSet<NswReleaseTitle> NswReleaseTitles { get; set; }


    public TitledbDbContext(DbContextOptions<TitledbDbContext> options) :
        base(options)
    {
       
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlite("Data Source=titles.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>()
            .HasMany(e => e.Languages)
            .WithOne(e => e.Category)
            .HasForeignKey(e => e.CategoryId);
        
        modelBuilder.Entity<Region>()
            .HasMany(e => e.Languages)
            .WithMany(e => e.Regions)
            .UsingEntity<RegionLanguage>();
        
        modelBuilder.Entity<Screenshot>()
            .HasOne<Title>(s => s.Title)
            .WithMany(t => t.Screenshots)
            .HasForeignKey(s => s.TitleId);
        
        modelBuilder.Entity<Screenshot>()
            .HasOne<Edition>(s => s.Edition)
            .WithMany(t => t.Screenshots)
            .HasForeignKey(s => s.EditionId);        
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Categories)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleCategory>();
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Editions)
            .WithOne(e => e.Title)
            .HasForeignKey(e => e.TitleId)
            .HasPrincipalKey(e => e.Id);
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Cnmts)
            .WithOne(e => e.Title)
            .HasForeignKey(e => e.TitleId)
            .HasPrincipalKey(e => e.Id);
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Languages)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleLanguage>();
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Regions)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleRegion>();
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.Versions)
            .WithOne(e => e.Title)
            .HasForeignKey(e => e.TitleId)
            .HasPrincipalKey(e => e.Id);
        
        modelBuilder.Entity<Title>()
            .HasMany(e => e.RatingContents)
            .WithMany(e => e.Titles)
            .UsingEntity<TitleRatingContent>();
        
    }
}