using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.DAL
{
    public enum FileSourceMembers
    {
        MusicLibrary,
        Other
    }

    public class PlaybackItem
    {
        public PlaybackItem()
        {
        }

        public PlaybackItem(string path, FileSourceMembers source)
        {
            Path = path;
            Source = source;
        }

        [Key]
        public string Path { get; set; }

        public FileSourceMembers Source { get; set; }
    }
}