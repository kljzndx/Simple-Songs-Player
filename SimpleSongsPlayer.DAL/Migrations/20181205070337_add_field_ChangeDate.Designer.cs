using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.DAL.Migrations
{
    [DbContext(typeof(FilesContext))]
    [Migration("20181205070337_add_field_ChangeDate")]
    partial class add_field_ChangeDate
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

                    b.Property<DateTime>("ChangeDate");

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

                    b.Property<string>("FilePath");

                    b.Property<string>("GroupName");

                    b.HasKey("Id");

                    b.ToTable("UserFavorites");
                });
        }
    }
}
