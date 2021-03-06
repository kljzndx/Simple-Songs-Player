﻿using Microsoft.EntityFrameworkCore;

namespace SimpleSongsPlayer.DAL
{
    public class FilesContext : DbContext
    {
        public DbSet<MusicFile> MusicFiles { get; set; }
        public DbSet<UserFavorite> UserFavorites { get; set; }
        public DbSet<LyricFile> LyricFiles { get; set; }
        public DbSet<LyricIndex> LyricIndices { get; set; }
        public DbSet<PlaybackItem> PlaybackList { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Files.db");
        }
    }
}