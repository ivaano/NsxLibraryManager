using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Models.NsxLibrary;
using NsxLibraryManager.Shared.Settings;
using Version = NsxLibraryManager.Models.NsxLibrary.Version;

namespace NsxLibraryManager.Data;

public class NsxLibraryDbContext : DbContext, IDataProtectionKeyContext
{
    private readonly UserSettings _configuration;
    public DbSet<Category> Categories { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<CategoryLanguage> CategoryLanguages { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<RatingsContent> RatingsContent { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<Title> Titles { get; set; }
    public DbSet<TitleCategory> TitleCategory { get; set; }
    public DbSet<Version> Versions { get; set; }
    public DbSet<LibraryUpdate> LibraryUpdates { get; set; }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

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
        
        modelBuilder.Entity<Title>()
            .Property(e => e.CreatedAt)
            .IsRequired();

        modelBuilder.Entity<Title>()
            .Property(e => e.UpdatedAt)
            .IsRequired();
    }
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker
            .Entries()
            .Where(e => e.Entity.GetType().GetProperties()
                            .Any(p => p.Name is "CreatedAt" or "UpdatedAt") &&
                        e.State is EntityState.Added or EntityState.Modified);

        foreach (var entityEntry in entries)
        {
            var createdAtProperty = entityEntry.Entity.GetType().GetProperty("CreatedAt");
            var updatedAtProperty = entityEntry.Entity.GetType().GetProperty("UpdatedAt");

            if (entityEntry.State == EntityState.Added && createdAtProperty != null)
            {
                createdAtProperty.SetValue(entityEntry.Entity, DateTime.UtcNow);
            }

            if (updatedAtProperty != null)
            {
                updatedAtProperty.SetValue(entityEntry.Entity, DateTime.UtcNow);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
    
}