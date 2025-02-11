using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsxLibraryManager.Migrations.NsxLibrary
{
    /// <inheritdoc />
    public partial class AddUserratingToLibraryTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Favorite",
                table: "Titles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "Notes",
                table: "Titles",
                type: "VARCHAR",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserRating",
                table: "Titles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Favorite",
                table: "Titles");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Titles");

            migrationBuilder.DropColumn(
                name: "UserRating",
                table: "Titles");
        }
    }
}
