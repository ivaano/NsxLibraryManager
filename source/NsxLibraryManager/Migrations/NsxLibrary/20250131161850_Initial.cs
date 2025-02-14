using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsxLibraryManager.Migrations.NsxLibrary
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Languages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LanguageCode = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Languages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibraryUpdates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    BaseTitleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdateTitleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    DlcTitleCount = table.Column<int>(type: "INTEGER", nullable: false),
                    LibraryPath = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryUpdates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RatingsContent",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingsContent", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Key = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OwnedUpdates = table.Column<int>(type: "INTEGER", nullable: true),
                    OwnedDlcs = table.Column<int>(type: "INTEGER", nullable: true),
                    Version = table.Column<int>(type: "INTEGER", nullable: true),
                    LatestOwnedUpdateVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    PackageType = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: false),
                    LastWriteTime = table.Column<DateTime>(type: "TEXT", nullable: true),
                    NsuId = table.Column<long>(type: "VARCHAR", maxLength: 200, nullable: true),
                    ApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    OtherApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: true),
                    TitleName = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    Intro = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    IconUrl = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    BannerUrl = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    Developer = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: true),
                    Publisher = table.Column<string>(type: "VARCHAR", maxLength: 50, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Rating = table.Column<int>(type: "INTEGER", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: true),
                    NumberOfPlayers = table.Column<int>(type: "INTEGER", nullable: true),
                    Region = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: true),
                    LatestVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    UpdatesCount = table.Column<int>(type: "INTEGER", nullable: true),
                    DlcCount = table.Column<int>(type: "INTEGER", nullable: true),
                    IsDemo = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContentType = table.Column<byte>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoryLanguages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Region = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: false),
                    Language = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: false),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryLanguages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CategoryLanguages_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LanguageTitle",
                columns: table => new
                {
                    LanguagesId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitlesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LanguageTitle", x => new { x.LanguagesId, x.TitlesId });
                    table.ForeignKey(
                        name: "FK_LanguageTitle_Languages_LanguagesId",
                        column: x => x.LanguagesId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LanguageTitle_Titles_TitlesId",
                        column: x => x.TitlesId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RatingsContentTitle",
                columns: table => new
                {
                    RatingsContentsId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitlesId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingsContentTitle", x => new { x.RatingsContentsId, x.TitlesId });
                    table.ForeignKey(
                        name: "FK_RatingsContentTitle_RatingsContent_RatingsContentsId",
                        column: x => x.RatingsContentsId,
                        principalTable: "RatingsContent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatingsContentTitle_Titles_TitlesId",
                        column: x => x.TitlesId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Screenshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: true),
                    EditionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screenshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Screenshots_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TitleCategory",
                columns: table => new
                {
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false),
                    CategoryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleCategory", x => new { x.CategoryId, x.TitleId });
                    table.ForeignKey(
                        name: "FK_TitleCategory_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TitleCategory_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Versions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VersionNumber = table.Column<int>(type: "INTEGER", nullable: false),
                    VersionDate = table.Column<string>(type: "TEXT", nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Versions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Versions_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLanguages_CategoryId",
                table: "CategoryLanguages",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_LanguageTitle_TitlesId",
                table: "LanguageTitle",
                column: "TitlesId");

            migrationBuilder.CreateIndex(
                name: "IX_RatingsContentTitle_TitlesId",
                table: "RatingsContentTitle",
                column: "TitlesId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenshots_TitleId",
                table: "Screenshots",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleCategory_TitleId",
                table: "TitleCategory",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Versions_TitleId",
                table: "Versions",
                column: "TitleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryLanguages");

            migrationBuilder.DropTable(
                name: "LanguageTitle");

            migrationBuilder.DropTable(
                name: "LibraryUpdates");

            migrationBuilder.DropTable(
                name: "RatingsContentTitle");

            migrationBuilder.DropTable(
                name: "Screenshots");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "TitleCategory");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "RatingsContent");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
