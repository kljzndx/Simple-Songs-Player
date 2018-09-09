using System;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public static class PlayItemChangeNotifier
    {
        public static event EventHandler<PlayItemChangeEventArgs> ItemChanged;

        public static void SendChangeNotification(Song song)
        {
            ItemChanged?.Invoke(null, new PlayItemChangeEventArgs(song.FileName, song.Title, song.Singer, song.Album, song.AlbumCover));
        }
    }
}