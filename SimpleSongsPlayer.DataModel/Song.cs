using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.DataModel
{
    public class Song
    {
        private Song(string title, string singer, string album, BitmapSource albumCover, TimeSpan duration, StorageFile file)
        {
            FileName = file.DisplayName;
            Title = title;
            Singer = "未知歌手";
            Album = "未知专辑";
            AlbumCover = albumCover;
            Duration = duration;
            File = file;

            if (!String.IsNullOrWhiteSpace(singer))
                Singer = singer;
            if (!String.IsNullOrWhiteSpace(album))
                Album = album;
        }

        public string FileName { get; }
        public string Title { get; }
        public string Singer { get; }
        public string Album { get; }
        public BitmapSource AlbumCover { get; }
        public TimeSpan Duration { get; }
        public StorageFile File { get; }

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