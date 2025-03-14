using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsxLibraryManager.Migrations.NsxLibrary
{
    /// <inheritdoc />
    public partial class AddDateAddedUpdatedToLibraryTitle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Titles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Titles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql(@"
            UPDATE Titles 
            SET CreatedAt = strftime('%Y-%m-%d %H:%M:%f', 'now'), 
                UpdatedAt = strftime('%Y-%m-%d %H:%M:%f', 'now')");
            
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Titles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Titles");
        }
    }
}
