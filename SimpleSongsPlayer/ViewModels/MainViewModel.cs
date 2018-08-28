using System.Collections.Generic;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public List<Song> AllSongs { get; set; }
        public List<LyricBlock> AllLyricBlocks { get; set; }
    }
}