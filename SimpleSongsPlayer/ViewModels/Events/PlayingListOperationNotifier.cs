using System;
using System.Collections.Generic;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public static class PlayingListOperationNotifier
    {
        public static event EventHandler<PlayingListAdditionRequestedEventArgs> AdditionRequested;

        public static void RequestAdd(IEnumerable<SongItem> songs)
        {
            AdditionRequested?.Invoke(null, new PlayingListAdditionRequestedEventArgs(songs));
        }
    }
}