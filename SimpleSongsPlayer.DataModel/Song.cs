﻿using System;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
        private static readonly ResourceLoader SongListStrings = ResourceLoader.GetForCurrentView("SongList");

        private bool isPlaying;
        private bool isSelected;
        
        private Song(StorageFolder baseFolder, StorageFile file, MusicProperties musicProperties, StorageItemThumbnail coverStream)
        {
            FolderName = baseFolder.DisplayName;
            FileName = file.DisplayName;
            Title = musicProperties.Title;
            Singer = SongListStrings.GetString("UnknownSinger");
            Album = SongListStrings.GetString("UnknownAlbum");
            CoverStream = coverStream;

            AlbumCover = new BitmapImage();
            AlbumCover.SetSource(coverStream);

            Duration = musicProperties.Duration;
            PlaybackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(file));

            if (!String.IsNullOrWhiteSpace(musicProperties.Artist))
                Singer = musicProperties.Artist;
            if (!String.IsNullOrWhiteSpace(musicProperties.Album))
                Album = musicProperties.Album;
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

        public string FolderName { get; }
        public string FileName { get; }
        public string Title { get; }
        public string Singer { get; }
        public string Album { get; }
        public StorageItemThumbnail CoverStream { get; }
        public BitmapSource AlbumCover { get; }
        public TimeSpan Duration { get; }
        public MediaPlaybackItem PlaybackItem { get; }

        public static async Task<Song> CreateFromStorageFile(StorageFile file)
        {
            var baseFolder = await file.GetParentAsync();
            var property = await file.Properties.GetMusicPropertiesAsync();
            var coverSource = await file.GetThumbnailAsync(ThumbnailMode.SingleItem);

            return new Song(baseFolder, file, property, coverSource);
        }
    }
}