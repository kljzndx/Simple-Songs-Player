using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSongsPlayer.DAL.Migrations
{
    public partial class AddNameField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "MusicFiles",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "LyricFiles",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileName",
                table: "MusicFiles");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "LyricFiles");
        }
    }
}
