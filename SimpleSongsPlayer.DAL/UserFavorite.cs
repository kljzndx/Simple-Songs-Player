using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleSongsPlayer.DAL
{
    public class UserFavorite
    {
        public UserFavorite()
        {
            
        }

        public UserFavorite(string groupName, string filePath)
        {
            GroupName = groupName;
            FilePath = filePath;
        }

        public int Id { get; set; }
        public string GroupName { get; set; }
        public string FilePath { get; set; }
    }
}