using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.DAL.Migrations
{
    [DbContext(typeof(FilesContext))]
    [Migration("20181119015736_AddTable_UserFavorites")]
    partial class AddTable_UserFavorites
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.6");

            modelBuilder.Entity("SimpleSongsPlayer.DAL.MusicFile", b =>
                {
                    b.Property<string>("Path")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Album");

                    b.Property<string>("Artist");

                    b.Property<TimeSpan>("Duration");

                    b.Property<string>("LibraryFolder")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired();

                    b.HasKey("Path");

                    b.ToTable("MusicFiles");
                });

            modelBuilder.Entity("SimpleSongsPlayer.DAL.UserFavorite", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("GroupName");

                    b.Property<string>("Path");

                    b.HasKey("Id");

                    b.HasIndex("Path");

                    b.ToTable("UserFavorites");
                });

            modelBuilder.Entity("SimpleSongsPlayer.DAL.UserFavorite", b =>
                {
                    b.HasOne("SimpleSongsPlayer.DAL.MusicFile", "File")
                        .WithMany()
                        .HasForeignKey("Path");
                });
        }
    }
}
