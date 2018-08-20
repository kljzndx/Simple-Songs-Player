using Windows.Storage.Pickers;

namespace SimpleSongsPlayer.Operator.Pickers
{
    public class MusicFileOpenPicker : FileOpenPickerBase
    {
        public MusicFileOpenPicker() : base(PickerLocationId.MusicLibrary, "aac", "wav", "flac", "alac", "m4a", "mp3")
        {
        }
    }
}