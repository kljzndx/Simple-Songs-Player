using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.Models.DTO
{
    public class MusicFileDTO
    {
        private StorageFile _file;

        public MusicFileDTO(MusicFile fileData)
        {
            Title = fileData.Title;
            Artist = fileData.Artist;
            Album = fileData.Album;
            Duration = fileData.Duration;
            FilePath = fileData.Path;
        }

        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public TimeSpan Duration { get; }
        public string FilePath { get; }

        public async Task<StorageFile> GetFile()
        {
            if (_file is null)
                _file = await StorageFile.GetFileFromPathAsync(FilePath);

            return _file;
        }

        public async Task<BitmapSource> GetAlbumCover()
        {
            var file = await GetFile();
            var bitmap = new BitmapImage();
            bitmap.SetSource(await file.GetThumbnailAsync(ThumbnailMode.SingleItem));
            return bitmap;
        }

        public override bool Equals(object obj)
        {
            if (obj is MusicFile data)
                return data.Path == FilePath;

            return base.Equals(obj);
        }
    }
}