using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSongsPlayer.DAL.Migrations
{
    public partial class AddParentColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DBVersion",
                table: "MusicFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentFolder",
                table: "MusicFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DBVersion",
                table: "LyricFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ParentFolder",
                table: "LyricFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DBVersion",
                table: "MusicFiles");

            migrationBuilder.DropColumn(
                name: "ParentFolder",
                table: "MusicFiles");

            migrationBuilder.DropColumn(
                name: "DBVersion",
                table: "LyricFiles");

            migrationBuilder.DropColumn(
                name: "ParentFolder",
                table: "LyricFiles");
        }
    }
}
