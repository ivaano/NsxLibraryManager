using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NsxLibraryManager.Migrations.Titledb
{
    /// <inheritdoc />
    public partial class Initial_Migration : Migration
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
                name: "RatingContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Regions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "VARCHAR", maxLength: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Regions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NsuId = table.Column<long>(type: "VARCHAR", maxLength: 200, nullable: true),
                    ApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
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
                    ContentType = table.Column<byte>(type: "INTEGER", nullable: false),
                    OtherApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: true)
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
                name: "RegionLanguage",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    RegionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegionLanguage", x => new { x.LanguageId, x.RegionId });
                    table.ForeignKey(
                        name: "FK_RegionLanguage_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegionLanguage_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cnmts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OtherApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: true),
                    RequiredApplicationVersion = table.Column<int>(type: "INTEGER", nullable: true),
                    TitleType = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cnmts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cnmts_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Editions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    NsuId = table.Column<long>(type: "VARCHAR", maxLength: 200, nullable: true),
                    TitleName = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    BannerUrl = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 5000, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: true),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Editions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Editions_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "TitleLanguages",
                columns: table => new
                {
                    LanguageId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleLanguages", x => new { x.LanguageId, x.TitleId });
                    table.ForeignKey(
                        name: "FK_TitleLanguages_Languages_LanguageId",
                        column: x => x.LanguageId,
                        principalTable: "Languages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TitleLanguages_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TitleRatingContents",
                columns: table => new
                {
                    RatingContentId = table.Column<int>(type: "INTEGER", nullable: false),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleRatingContents", x => new { x.RatingContentId, x.TitleId });
                    table.ForeignKey(
                        name: "FK_TitleRatingContents_RatingContents_RatingContentId",
                        column: x => x.RatingContentId,
                        principalTable: "RatingContents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TitleRatingContents_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TitleRegion",
                columns: table => new
                {
                    TitleId = table.Column<int>(type: "INTEGER", nullable: false),
                    RegionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TitleRegion", x => new { x.RegionId, x.TitleId });
                    table.ForeignKey(
                        name: "FK_TitleRegion_Regions_RegionId",
                        column: x => x.RegionId,
                        principalTable: "Regions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TitleRegion_Titles_TitleId",
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

            migrationBuilder.CreateTable(
                name: "Screenshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Url = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true),
                    TitleId = table.Column<int>(type: "INTEGER", nullable: true),
                    EditionId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Screenshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Screenshots_Editions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "Editions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Screenshots_Titles_TitleId",
                        column: x => x.TitleId,
                        principalTable: "Titles",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "Languages",
                columns: new[] { "Id", "LanguageCode" },
                values: new object[,]
                {
                    { 1, "en" },
                    { 2, "pt" },
                    { 3, "fr" },
                    { 4, "de" },
                    { 5, "it" },
                    { 6, "es" },
                    { 7, "ko" },
                    { 8, "zh" },
                    { 9, "nl" },
                    { 10, "ru" },
                    { 11, "ja" }
                });

            migrationBuilder.InsertData(
                table: "Regions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "BG" },
                    { 2, "BR" },
                    { 3, "CH" },
                    { 4, "CY" },
                    { 5, "EE" },
                    { 6, "HR" },
                    { 7, "IE" },
                    { 8, "LT" },
                    { 9, "LU" },
                    { 10, "LV" },
                    { 11, "MT" },
                    { 12, "RO" },
                    { 13, "SI" },
                    { 14, "SK" },
                    { 15, "CO" },
                    { 16, "AR" },
                    { 17, "CL" },
                    { 18, "PE" },
                    { 19, "KR" },
                    { 20, "HK" },
                    { 21, "CN" },
                    { 22, "NZ" },
                    { 23, "AT" },
                    { 24, "BE" },
                    { 25, "CZ" },
                    { 26, "DK" },
                    { 27, "ES" },
                    { 28, "FI" },
                    { 29, "GR" },
                    { 30, "HU" },
                    { 31, "NL" },
                    { 32, "NO" },
                    { 33, "PL" },
                    { 34, "PT" },
                    { 35, "RU" },
                    { 36, "ZA" },
                    { 37, "SE" },
                    { 38, "MX" },
                    { 39, "IT" },
                    { 40, "CA" },
                    { 41, "FR" },
                    { 42, "DE" },
                    { 43, "JP" },
                    { 44, "AU" },
                    { 45, "GB" },
                    { 46, "US" }
                });

            migrationBuilder.InsertData(
                table: "RegionLanguage",
                columns: new[] { "LanguageId", "RegionId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 },
                    { 1, 10 },
                    { 1, 11 },
                    { 1, 12 },
                    { 1, 13 },
                    { 1, 14 },
                    { 1, 15 },
                    { 1, 16 },
                    { 1, 17 },
                    { 1, 18 },
                    { 1, 22 },
                    { 1, 25 },
                    { 1, 26 },
                    { 1, 28 },
                    { 1, 29 },
                    { 1, 30 },
                    { 1, 32 },
                    { 1, 33 },
                    { 1, 36 },
                    { 1, 37 },
                    { 1, 38 },
                    { 1, 40 },
                    { 1, 44 },
                    { 1, 45 },
                    { 1, 46 },
                    { 2, 2 },
                    { 2, 34 },
                    { 3, 3 },
                    { 3, 9 },
                    { 3, 24 },
                    { 3, 40 },
                    { 3, 41 },
                    { 4, 3 },
                    { 4, 9 },
                    { 4, 23 },
                    { 4, 42 },
                    { 5, 3 },
                    { 5, 39 },
                    { 6, 15 },
                    { 6, 16 },
                    { 6, 17 },
                    { 6, 18 },
                    { 6, 27 },
                    { 6, 38 },
                    { 6, 46 },
                    { 7, 19 },
                    { 8, 20 },
                    { 8, 21 },
                    { 9, 24 },
                    { 9, 31 },
                    { 10, 35 },
                    { 11, 43 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryLanguages_CategoryId",
                table: "CategoryLanguages",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Cnmts_TitleId",
                table: "Cnmts",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_Editions_TitleId",
                table: "Editions",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_RegionLanguage_RegionId",
                table: "RegionLanguage",
                column: "RegionId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenshots_EditionId",
                table: "Screenshots",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_Screenshots_TitleId",
                table: "Screenshots",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleCategory_TitleId",
                table: "TitleCategory",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleLanguages_TitleId",
                table: "TitleLanguages",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleRatingContents_TitleId",
                table: "TitleRatingContents",
                column: "TitleId");

            migrationBuilder.CreateIndex(
                name: "IX_TitleRegion_TitleId",
                table: "TitleRegion",
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
                name: "Cnmts");

            migrationBuilder.DropTable(
                name: "RegionLanguage");

            migrationBuilder.DropTable(
                name: "Screenshots");

            migrationBuilder.DropTable(
                name: "TitleCategory");

            migrationBuilder.DropTable(
                name: "TitleLanguages");

            migrationBuilder.DropTable(
                name: "TitleRatingContents");

            migrationBuilder.DropTable(
                name: "TitleRegion");

            migrationBuilder.DropTable(
                name: "Versions");

            migrationBuilder.DropTable(
                name: "Editions");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Languages");

            migrationBuilder.DropTable(
                name: "RatingContents");

            migrationBuilder.DropTable(
                name: "Regions");

            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
