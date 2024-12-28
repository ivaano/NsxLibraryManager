﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NsxLibraryManager.Core.Settings;
using NsxLibraryManager.Models.NsxLibrary;

namespace NsxLibraryManager.Data;

public class NsxLibraryDbContext : DbContext
{
    private readonly AppSettings _configuration;
    public DbSet<Title> Titles { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<TitleCategory> TitleCategory { get; set; }
    public DbSet<Language> Languages { get; set; }
    public DbSet<Region> Regions { get; set; }
    public DbSet<Screenshot> Screenshots { get; set; }

    public DbSet<CategoryLanguage> CategoryLanguages { get; set; }


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