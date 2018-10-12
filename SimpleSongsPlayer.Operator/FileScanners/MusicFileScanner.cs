using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace SimpleSongsPlayer.Operator.FileScanners
{
    public class MusicFileScanner : FileScannerBase
    {
        public MusicFileScanner() : base("aac", "wav", "flac", "alac", "m4a", "mp3")
        {
            string[] otherProperties =
            {
                SystemProperties.Music.Artist,
                SystemProperties.Title,
                SystemProperties.Music.AlbumTitle
            };
            QueryOptions.IndexerOption = IndexerOption.OnlyUseIndexerAndOptimizeForIndexedProperties;
            QueryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.MusicProperties, otherProperties);
        }
    }
}