using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.DAL.Migrations
{
    [DbContext(typeof(FilesContext))]
    [Migration("20190208095309_AddTable_PlaybackList")]
    partial class AddTable_PlaybackList
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.6");

            modelBuilder.Entity("SimpleSongsPlayer.DAL.LyricFile", b =>
                {
                    b.Property<string>("Path")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("ChangeDate");

                    b.Property<string>("DBVersion");

                    b.Property<string>("FileName");

                    b.Property<string>("LibraryFolder");

                    b.Property<string>("ParentFolder");

                    b.HasKey("Path");

                    b.ToTable("LyricFiles");
                });

            modelBuilder.Entity("SimpleSongsPlayer.DAL.LyricIndex", b =>
                {
                    b.Property<string>("MusicPath")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("LyricPath")
                        .IsRequired();

                    b.HasKey("MusicPath");

                    b.ToTable("LyricIndices");
                });

            modelBuilder.Entity("SimpleSongsPlayer.DAL.MusicFile", b =>
                {
                    b.Property<string>("Path")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Album");

                    b.Property<string>("Artist");

                    b.Property<DateTime>("ChangeDate");

                    b.Property<string>("DBVersion");

                    b.Property<TimeSpan>("Duration");

                    b.Property<string>("FileName");

                    b.Property<string>("LibraryFolder")
                        .IsRequired();

                    b.Property<string>("ParentFolder");

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<uint>("TrackNumber");

                    b.HasKey("Path");

                    b.ToTable("MusicFiles");
                });

            modelBuilder.Entity("SimpleSongsPlayer.DAL.PlaybackItem", b =>
                {
                    b.Property<string>("Path")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Source");

                    b.HasKey("Path");

                    b.ToTable("PlaybackList");
                });

            modelBuilder.Entity("SimpleSongsPlayer.DAL.UserFavorite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("FilePath");

                    b.Property<string>("GroupName");

                    b.HasKey("Id");

                    b.ToTable("UserFavorites");
                });
        }
    }
}
