using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.Models.DTO
{
    public class MusicFileDTO : ObservableObject
    {
        private WeakReference<StorageFile> _fileReference = new WeakReference<StorageFile>(null);
        private MediaPlaybackItem _playbackItem;

        private bool isPlaying;
        
        public MusicFileDTO(MusicFile fileData)
        {
            Title = fileData.Title;
            Artist = fileData.Artist;
            Album = fileData.Album;
            Duration = fileData.Duration;
            FilePath = fileData.Path;
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set => Set(ref isPlaying, value);
        }

        public string Title { get; }
        public string Artist { get; }
        public string Album { get; }
        public TimeSpan Duration { get; }
        public string FilePath { get; }

        private async Task<StorageFile> GetFile()
        {
            StorageFile file = null;
            if (_fileReference.TryGetTarget(out file))
                return file;

            file = await StorageFile.GetFileFromPathAsync(FilePath);
            _fileReference.SetTarget(file);
            return file;
        }

        public async Task<BitmapSource> GetAlbumCover()
        {
            var file = await GetFile();
            var bitmap = new BitmapImage();
            bitmap.SetSource(await file.GetThumbnailAsync(ThumbnailMode.SingleItem));
            return bitmap;
        }

        public async Task<MediaPlaybackItem> GetPlaybackItem()
        {
            if (_playbackItem is null)
                _playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await GetFile()));

            return _playbackItem;
        }
    }
}