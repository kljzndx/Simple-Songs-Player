using Windows.Storage.Pickers;

namespace SimpleSongsPlayer.Operator.Pickers
{
    public class LyricsFileOpenPicker : FileOpenPickerBase
    {
        public LyricsFileOpenPicker() : base(PickerLocationId.MusicLibrary, "lrc")
        {
        }
    }
}