using System.ComponentModel.DataAnnotations;

namespace SimpleSongsPlayer.Dal
{
    public class PlaybackItem
    {
        public PlaybackItem() { }

        public PlaybackItem(int musicFileId, int trackId = 0)
        {
            MusicFileId = musicFileId;
            TrackId = trackId;
        }

        [Key]
        public int PlaybackItemId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public int MusicFileId { get; set; }

        [Required(AllowEmptyStrings = false)]
        public int TrackId { get; set; }
        public bool IsPlaying { get; set; }

        public MusicFile File { get; set; }
    }
}
