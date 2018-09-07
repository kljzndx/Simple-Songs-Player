using Windows.Media.Playback;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Controllers
{
    public class PlayerControllerViewModel : ViewModelBase
    {
        private MediaPlaybackList playerSource;
        private Song currentSong;

        public MediaPlaybackList PlayerSource
        {
            get => playerSource;
            set => Set(ref playerSource, value);
        }

        public Song CurrentSong
        {
            get => currentSong;
            set => Set(ref currentSong, value);
        }


    }
}