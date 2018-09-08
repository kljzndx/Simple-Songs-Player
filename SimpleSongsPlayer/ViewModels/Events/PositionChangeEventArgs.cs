using System;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PositionChangeEventArgs : EventArgs
    {
        public PositionChangeEventArgs(bool didUserChange, TimeSpan position)
        {
            DidUserChange = didUserChange;
            Position = position;
        }

        public bool DidUserChange { get; }
        public TimeSpan Position { get; }
    }
}