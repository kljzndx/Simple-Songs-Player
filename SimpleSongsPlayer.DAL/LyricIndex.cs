using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.DAL
{
    public class LyricIndex
    {
        public LyricIndex()
        {
        }

        public LyricIndex(string musicPath, string lyricPath)
        {
            MusicPath = musicPath;
            LyricPath = lyricPath;
        }

        [Key]
        [Required(AllowEmptyStrings = false)]
        public string MusicPath { get; set; }
        [Required(AllowEmptyStrings = false)]
        public string LyricPath { get; set; }
    }
}