namespace SimpleSongsPlayer.Operator.FileScanners
{
    public class MusicFileScanner : FileScannerBase
    {
        public MusicFileScanner() : base("aac", "wav", "flac", "alac", "m4a", "mp3")
        {
        }
    }
}