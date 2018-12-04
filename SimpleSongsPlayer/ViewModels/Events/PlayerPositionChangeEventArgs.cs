using System;

namespace SimpleSongsPlayer.ViewModels.Events
{
    public class PlayerPositionChangeEventArgs : EventArgs
    {
        public PlayerPositionChangeEventArgs(bool isUser, TimeSpan position)
        {
            IsUser = isUser;
            Position = position;
        }

        public bool IsUser { get; }
        public TimeSpan Position { get; }
    }
}