using System;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PlayItemChangeEventArgs : EventArgs
    {
        public PlayItemChangeEventArgs(string fileName, string title, string artist, string album, BitmapSource albumCover)
        {
            FileName = fileName;
            Title = title;
            Artist = artist;
            Album = album;
            AlbumCover = albumCover;
        }

        public string FileName { get; }
        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public BitmapSource AlbumCover { get; }
    }
}