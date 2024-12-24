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
                name: "Titles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NsuId = table.Column<long>(type: "VARCHAR", maxLength: 200, nullable: true),
                    ApplicationId = table.Column<string>(type: "VARCHAR", maxLength: 20, nullable: false),
                    TitleName = table.Column<string>(type: "VARCHAR", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Titles", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Titles");
        }
    }
}
