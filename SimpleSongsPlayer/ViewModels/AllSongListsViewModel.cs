using System.Collections.Generic;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels
{
    public class AllSongListsViewModel : ViewModelBase
    {
        public List<Song> AllSongs { get; set; }
    }
}