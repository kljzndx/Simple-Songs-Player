using System;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public static class PositionChangeNotifier
    {
        public static event EventHandler<PositionChangeEventArgs> PositionChanged;

        public static void SendChangeNotification(bool didUserChange, TimeSpan position)
        {
            PositionChanged?.Invoke(null, new PositionChangeEventArgs(didUserChange, position));
        }
    }
}