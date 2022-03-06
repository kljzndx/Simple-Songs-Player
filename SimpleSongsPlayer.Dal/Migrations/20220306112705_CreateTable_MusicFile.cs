using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSongsPlayer.Dal.Migrations
{
    public partial class CreateTable_MusicFile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MusicFiles",
                columns: table => new
                {
                    Index = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(nullable: true),
                    Artist = table.Column<string>(nullable: true),
                    Album = table.Column<string>(nullable: true),
                    TrackNumber = table.Column<uint>(nullable: false),
                    Duration = table.Column<TimeSpan>(nullable: false),
                    FilePath = table.Column<string>(nullable: false),
                    LibraryFolder = table.Column<string>(nullable: false),
                    FileChangeDate = table.Column<DateTime>(nullable: false),
                    DbVersion = table.Column<string>(nullable: true, defaultValue: "V1")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicFiles", x => x.Index);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicFiles_FilePath",
                table: "MusicFiles",
                column: "FilePath");

            migrationBuilder.CreateIndex(
                name: "IX_MusicFiles_LibraryFolder",
                table: "MusicFiles",
                column: "LibraryFolder");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicFiles");
        }
    }
}
