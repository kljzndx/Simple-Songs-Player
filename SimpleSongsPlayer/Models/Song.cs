using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.Models
{
    public class Song
    {
        private Song(string name, string singer, string album, BitmapSource albumCover, TimeSpan duration, StorageFile file)
        {
            Name = name;
            Singer = singer;
            Album = album;
            AlbumCover = albumCover;
            Duration = duration;
            File = file;
        }

        public string Name { get; set; }
        public string Singer { get; set; }
        public string Album { get; set; }
        public BitmapSource AlbumCover { get; set; }
        public TimeSpan Duration { get; set; }
        public StorageFile File { get; set; }

        public static async Task<Song> CreateFromStorageFile(StorageFile file)
        {
            var property = await file.Properties.GetMusicPropertiesAsync();
            BitmapImage cover = new BitmapImage();
            cover.SetSource(await file.GetThumbnailAsync(ThumbnailMode.MusicView));

            return new Song(String.IsNullOrWhiteSpace(property.Title) ? file.DisplayName : property.Title,
                property.Artist, property.Album, cover, property.Duration, file);
        }
    }
}