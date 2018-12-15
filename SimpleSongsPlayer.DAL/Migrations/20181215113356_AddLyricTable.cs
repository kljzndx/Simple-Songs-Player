using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSongsPlayer.DAL.Migrations
{
    public partial class AddLyricTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LyricFiles",
                columns: table => new
                {
                    Path = table.Column<string>(nullable: false),
                    ChangeDate = table.Column<DateTime>(nullable: false),
                    LibraryFolder = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LyricFiles", x => x.Path);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LyricFiles");
        }
    }
}
