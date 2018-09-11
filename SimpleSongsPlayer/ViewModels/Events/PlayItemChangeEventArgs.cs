using System;
using Windows.UI.Xaml.Media.Imaging;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PlayItemChangeEventArgs : EventArgs
    {
        public PlayItemChangeEventArgs(Song song)
        {
            Song = song;
        }

        public Song Song { get; }
    }
}