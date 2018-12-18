using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.DAL
{
    public class LyricIndex
    {
        [Key]
        [Required(AllowEmptyStrings = false)]
        public string MusicPath { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string LyricPath { get; set; }
    }
}