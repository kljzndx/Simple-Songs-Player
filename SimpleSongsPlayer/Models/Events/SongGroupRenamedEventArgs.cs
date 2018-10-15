using System;

namespace SimpleSongsPlayer.Models.Events
{
    public class SongGroupRenamedEventArgs : EventArgs
    {
        public SongGroupRenamedEventArgs(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }

        public string OldName { get; }
        public string NewName { get; }
    }
}