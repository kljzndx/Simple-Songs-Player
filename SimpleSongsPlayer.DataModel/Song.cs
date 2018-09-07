using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.DataModel
{
    public class Song : ObservableObject
    {
        private bool isPlaying;
        private bool isSelected;

        private Song(string fileName, string title, string singer, string album, BitmapSource albumCover, TimeSpan duration, MediaPlaybackItem playbackItem)
        {
            FileName = fileName;
            Title = title;
            Singer = "未知歌手";
            Album = "未知专辑";
            AlbumCover = albumCover;
            Duration = duration;
            PlaybackItem = playbackItem;

            if (!String.IsNullOrWhiteSpace(singer))
                Singer = singer;
            if (!String.IsNullOrWhiteSpace(album))
                Album = album;
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set => Set(ref isPlaying, value);
        }

        public bool IsSelected
        {
            get => isSelected;
            set => Set(ref isSelected, value);
        }

        public string FileName { get; }
        public string Title { get; }
        public string Singer { get; }
        public string Album { get; }
        public BitmapSource AlbumCover { get; }
        public TimeSpan Duration { get; }
        public MediaPlaybackItem PlaybackItem { get; }

        public static async Task<Song> CreateFromStorageFile(StorageFile file)
        {
            var property = await file.Properties.GetMusicPropertiesAsync();

            var coverSource = await file.GetThumbnailAsync(ThumbnailMode.MusicView);
            BitmapImage cover = new BitmapImage();
            cover.SetSource(coverSource);

            MediaPlaybackItem playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));
            var mediaProperties = playbackItem.GetDisplayProperties();
            mediaProperties.Thumbnail = RandomAccessStreamReference.CreateFromStream(coverSource);

            // 无法保存数据 --2018/09/04
            //var musicProperties = mediaProperties.MusicProperties;
            //musicProperties.Title = property.Title;
            //musicProperties.Artist = property.Artist;
            //musicProperties.AlbumTitle = property.Album;
            //musicProperties.AlbumArtist = property.AlbumArtist;

            playbackItem.ApplyDisplayProperties(mediaProperties);
            
            return new Song(file.DisplayName, String.IsNullOrWhiteSpace(property.Title) ? file.DisplayName : property.Title,
                property.Artist, property.Album, cover, property.Duration, playbackItem);
        }
    }
}