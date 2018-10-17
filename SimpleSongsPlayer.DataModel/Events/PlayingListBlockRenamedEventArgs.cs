namespace SimpleSongsPlayer.DataModel.Events
{
    public class PlayingListBlockRenamedEventArgs
    {
        public PlayingListBlockRenamedEventArgs(string oldName, string newName)
        {
            OldName = oldName;
            NewName = newName;
        }

        public string OldName { get; }
        public string NewName { get; }
    }
}