using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSongsPlayer.DAL
{
    public class UserFavorite
    {
        public int Id { get; set; }
        public string GroupName { get; set; }
        
        [ForeignKey("Path")]
        [Column("FilePath")]
        public MusicFile File { get; set; }
    }
}