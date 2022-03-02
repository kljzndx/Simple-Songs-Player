using Microsoft.EntityFrameworkCore;

namespace SimpleSongsPlayer.Dal
{
    public class MainContext : DbContext
    {
        public DbSet<MusicFile> MusicFiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MusicFile>().HasIndex(mf => mf.FilePath);
            modelBuilder.Entity<MusicFile>().HasIndex(mf => mf.LibraryFolder);
            modelBuilder.Entity<MusicFile>().HasIndex(mf => mf.IsInPlaybackList);

            modelBuilder.Entity<MusicFile>().Property(mf => mf.DbVersion).HasDefaultValue("V1");
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Main.db");
        }
    }
}
