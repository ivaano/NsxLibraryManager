using System.Reflection;
using LiteDB;
using NsxLibraryManager.Core.Enums;
using NsxLibraryManager.Core.Models;
using NsxLibraryManager.Core.Settings;

namespace Tests.Fixtures;

public class DatabaseFixture : IDisposable
{
    public DatabaseFixture()
    {
        DbPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Path.GetTempPath(), "nsx-library-manager-tests.db");
        File.Delete(DbPath);
        using (var db = new LiteDatabase(DbPath))
        {
            AddCnmt(db);
            AddRegionTitleDbTitle(db);
            AddLibraryTitles(db);
        }
    }

    private static void AddLibraryTitles(ILiteDatabase db)
    {
        var col = db.GetCollection<LibraryTitle>(AppConstants.LibraryCollectionName);

        var titles = new List<LibraryTitle>
        {
                new()
                {
                        Type = TitleLibraryType.Base,
                        TitleId = "B1",
                        FileName = "B1.nsp",
                        PackageType = AccuratePackageType.NSP,
                        Category = new List<string> { "Action", "Adventure" },
                },
                new()
                {
                        Type = TitleLibraryType.Base,
                        TitleId = "B2",
                        FileName = "B2.nsz",
                        PackageType = AccuratePackageType.NSZ,
                        Category = new List<string> { "Action", "Puzzle" },
                },
                new()
                {
                        Type = TitleLibraryType.Base,
                        TitleId = "B3",
                        FileName = "B3.nsz",
                        PackageType = AccuratePackageType.XCZ,
                        Category = new List<string> { "Figthing", "Action" },
                },
                new()
                {
                        Type = TitleLibraryType.Base,
                        TitleId = "B4",
                        FileName = "B4.nsz",
                        PackageType = AccuratePackageType.XCZ,
                        Category = new List<string> { "Platformer", "Action" },
                },
                new()
                {
                        Type = TitleLibraryType.Update,
                        TitleId = "U1",
                        FileName = "U1.nsz",
                        PackageType = AccuratePackageType.NSZ
                },
                new()
                {
                        Type = TitleLibraryType.DLC,
                        TitleId = "D1",
                        FileName = "D1.nsz",
                        PackageType = AccuratePackageType.NSZ,
                },
                new()
                {
                        Type = TitleLibraryType.DLC,
                        TitleId = "D2",
                        FileName = "D2.nsz",
                        PackageType = AccuratePackageType.NSZ,
                },
                new()
                {
                        Type = TitleLibraryType.DLC,
                        TitleId = "D3",
                        FileName = "D3.nsz",
                        PackageType = AccuratePackageType.NSZ,
                }
        };
        
        col.InsertBulk(titles);
        col.EnsureIndex(x => x.TitleId);
        db.Commit();
    }

    private static void AddRegionTitleDbTitle(ILiteDatabase db)
    {
        //us region for now
        var col = db.GetCollection<RegionTitle>("US");

        var title = new RegionTitle
        {
                TitleId = "1000000000000001",
                Category = new List<string> { "Action", "Adventure" },
                Description = "Test Description",
                IconUrl = "http://someurl.com/icon.jpg",
                Intro = "Intro perron",
                Type = TitleLibraryType.Base,
                Name = "Test Title",
                NumberOfPlayers = 1,
                Region = "US",
                ReleaseDate = DateTime.Today,
        };
        col.Insert(title);
        col.EnsureIndex(x => x.TitleId);
        col.EnsureIndex(x => x.Ids);
        db.Commit();
    }

    private static void AddCnmt(ILiteDatabase db)
    {
        var col = db.GetCollection<PackagedContentMeta>(AppConstants.CnmtsCollectionName);

        var cnmt = new PackagedContentMeta
        {
                OtherApplicationId = "0100000000001000",
                TitleId = "1000000000000001",
                TitleType = 130,
                Version = "1.0.0",
        };
        col.Insert(cnmt);
        col.EnsureIndex(x => x.TitleId);
        col.EnsureIndex(x => x.OtherApplicationId);
        db.Commit();
    }

    public void Dispose()
    {
        File.Delete(DbPath);
    }

    public string DbPath { get; set; }
    
}