using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSongsPlayer.DAL.Migrations
{
    public partial class Add_TrackNumber_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "TrackNumber",
                table: "MusicFiles",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TrackNumber",
                table: "MusicFiles");
        }
    }
}
