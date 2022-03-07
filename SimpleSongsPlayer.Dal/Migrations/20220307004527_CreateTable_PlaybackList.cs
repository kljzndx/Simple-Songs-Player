using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleSongsPlayer.Dal.Migrations
{
    public partial class CreateTable_PlaybackList : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaybackList",
                columns: table => new
                {
                    PlaybackItemId = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MusicFileId = table.Column<int>(nullable: false),
                    TrackId = table.Column<int>(nullable: false),
                    IsPlaying = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybackList", x => x.PlaybackItemId);
                    table.ForeignKey(
                        name: "FK_PlaybackList_MusicFiles_MusicFileId",
                        column: x => x.MusicFileId,
                        principalTable: "MusicFiles",
                        principalColumn: "MusicFileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackList_MusicFileId",
                table: "PlaybackList",
                column: "MusicFileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackList_TrackId",
                table: "PlaybackList",
                column: "TrackId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybackList");
        }
    }
}
