using System;
using System.Collections.Generic;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public static class PlayingListOperationNotifier
    {
        public static event EventHandler<PlayingListAdditionRequestedEventArgs> AdditionRequested;

        public static void RequestAdd(IEnumerable<Song> songs)
        {
            AdditionRequested?.Invoke(null, new PlayingListAdditionRequestedEventArgs(songs));
        }
    }
}