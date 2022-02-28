using Microsoft.EntityFrameworkCore;

namespace SimpleSongsPlayer.Dal
{
    public class MainContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=Main.db");
        }
    }
}
