using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PlayingListAdditionRequestedEventArgs : EventArgs
    {
        public PlayingListAdditionRequestedEventArgs(IEnumerable<Song> songs)
        {
            Paths = songs.Select(s => s.Path).ToList();
        }

        public List<string> Paths { get; }
    }
}