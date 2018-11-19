using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSongsPlayer.DAL
{
    public class UserFavorite
    {
        public UserFavorite()
        {
            
        }

        public UserFavorite(string groupName, MusicFile file)
        {
            GroupName = groupName;
            File = file;
        }

        public int Id { get; set; }
        public string GroupName { get; set; }
        
        [ForeignKey("Path")]
        [Column("FilePath")]
        public MusicFile File { get; set; }
    }
}