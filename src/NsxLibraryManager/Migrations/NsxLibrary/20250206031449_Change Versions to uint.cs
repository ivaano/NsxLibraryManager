using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsxLibraryManager.Migrations.NsxLibrary
{
    /// <inheritdoc />
    public partial class ChangeVersionstouint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<uint>(
                name: "Version",
                table: "Titles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "LatestVersion",
                table: "Titles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "LatestOwnedUpdateVersion",
                table: "Titles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0u,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Version",
                table: "Titles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "LatestVersion",
                table: "Titles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "LatestOwnedUpdateVersion",
                table: "Titles",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "INTEGER");
        }
    }
}
