using Windows.Media.Playback;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Controllers
{
    public class PlayerControllerViewModel : ViewModelBase
    {
        private Song currentSong;

        public MediaPlaybackList PlayerSource { get; set; }

        public Song CurrentSong
        {
            get => currentSong;
            set => Set(ref currentSong, value);
        }


    }
}