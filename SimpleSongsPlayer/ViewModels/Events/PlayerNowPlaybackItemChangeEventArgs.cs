using Windows.Media.Playback;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PlayerNowPlaybackItemChangeEventArgs
    {
        public PlayerNowPlaybackItemChangeEventArgs(MediaPlaybackItem oldItem, MediaPlaybackItem newItem)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }

        public MediaPlaybackItem OldItem { get; set; }
        public MediaPlaybackItem NewItem { get; set; }
    }
}