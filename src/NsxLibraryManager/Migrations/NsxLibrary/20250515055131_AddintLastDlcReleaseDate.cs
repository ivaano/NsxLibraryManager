﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NsxLibraryManager.Migrations.NsxLibrary
{
    /// <inheritdoc />
    public partial class AddintLastDlcReleaseDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LatestMissingDlcDate",
                table: "Titles",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatestMissingDlcDate",
                table: "Titles");
        }
    }
}
