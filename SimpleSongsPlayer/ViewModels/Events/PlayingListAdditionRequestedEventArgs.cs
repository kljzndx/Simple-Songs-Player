using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PlayingListAdditionRequestedEventArgs : EventArgs
    {
        public PlayingListAdditionRequestedEventArgs(IEnumerable<SongItem> songs)
        {
            Paths = songs.Select(s => s.Path).ToList();
        }

        public List<string> Paths { get; }
    }
}