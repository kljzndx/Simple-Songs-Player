using System.Collections.Generic;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels
{
    public class PlayingViewModel : ViewModelBase
    {
        private List<LyricBlock> allLyricBlocks;
        private Song currentSong;

        public List<LyricBlock> AllLyricBlocks
        {
            get => allLyricBlocks;
            set => Set(ref allLyricBlocks, value);
        }

        public Song CurrentSong
        {
            get => currentSong;
            set => Set(ref currentSong, value);
        }
    }
}