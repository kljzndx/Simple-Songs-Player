﻿using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.Models.DTO
{
    public class MusicFileDTO : ObservableObject
    {
        private static readonly MusicFileFactory Factory = new MusicFileFactory();

        private static readonly string UnknownArtist;
        private static readonly string UnknownAlbum;

        private WeakReference<StorageFile> _fileReference = new WeakReference<StorageFile>(null);
        private WeakReference<StorageItemThumbnail> _thumbnail = new WeakReference<StorageItemThumbnail>(null);
        private WeakReference<BitmapSource> _bitmap = new WeakReference<BitmapSource>(null);
        private MediaPlaybackItem _playbackItem;
        private StorageFile _file;

        private bool isPlaying;

        private uint trackNumber;
        private string title;
        private string artist;
        private string album;
        private TimeSpan duration;
        private DateTime changeDate;

        static MusicFileDTO()
        {
            ResourceLoader itemStrings = ResourceLoader.GetForCurrentView("MusicItem");
            UnknownArtist = itemStrings.GetString(nameof(UnknownArtist));
            UnknownAlbum = itemStrings.GetString(nameof(UnknownAlbum));
        }
        
        public MusicFileDTO(MusicFile fileData)
        {
            FileName = fileData.FileName;
            ParentFolderPath = fileData.ParentFolder;
            FilePath = fileData.Path;
            LibraryFolderPath = fileData.LibraryFolder;
            changeDate = fileData.ChangeDate;
            
            trackNumber = fileData.TrackNumber;
            title = fileData.Title;
            artist = fileData.Artist;
            album = fileData.Album;
            duration = fileData.Duration;
        }

        public MusicFileDTO(MusicFile fileData, StorageFile file) : this(fileData)
        {
            _file = file;
        }

        public bool IsPlaying
        {
            get => isPlaying;
            set => Set(ref isPlaying, value);
        }


        public uint TrackNumber
        {
            get => trackNumber;
            set => Set(ref trackNumber, value);
        }

        public string Title
        {
            get => title;
            private set => Set(ref title, value);
        }

        public string Artist
        {
            get => FoundArtist ? artist : UnknownArtist;
            private set => Set(ref artist, value);
        }

        public string Album
        {
            get => FoundAlbum ? album : UnknownAlbum;
            private set => Set(ref album, value);
        }

        public TimeSpan Duration
        {
            get => duration;
            private set => Set(ref duration, value);
        }

        public DateTime ChangeDate
        {
            get => changeDate;
            private set => Set(ref changeDate, value);
        }
        
        public string FileName { get; }
        public string ParentFolderPath { get; }
        public string FilePath { get; }
        public string LibraryFolderPath { get; }
        public string LibraryFolderName => LibraryFolderPath.Split('\\').Last();

        public bool FoundArtist => !String.IsNullOrWhiteSpace(artist);
        public bool FoundAlbum => !String.IsNullOrWhiteSpace(album);

        public bool IsInitPlaybackItem => _playbackItem != null;

        public void Update(MusicFile newData)
        {
            Title = newData.Title;
            Duration = newData.Duration;
            Artist = newData.Artist;
            Album = newData.Album;
            ChangeDate = newData.ChangeDate;
        }

        private async Task<StorageFile> GetFile()
        {
            if (_file != null)
                return _file;

            StorageFile file = null;
            if (_fileReference.TryGetTarget(out file))
                return file;

            file = await StorageFile.GetFileFromPathAsync(FilePath);
            _fileReference.SetTarget(file);
            return file;
        }

        public async Task<StorageFolder> GetLocation()
        {
            var file = await GetFile();
            return await file.GetParentAsync();
        }

        public async Task<StorageItemThumbnail> GetThumbnail()
        {
            StorageItemThumbnail thumbnail = null;
            if (_thumbnail.TryGetTarget(out thumbnail))
                return thumbnail;

            var file = await GetFile();
            thumbnail = await file.GetProperties(async f => await f.GetThumbnailAsync(ThumbnailMode.SingleItem));
            _thumbnail.SetTarget(thumbnail);

            return thumbnail;
        }

        public async Task<BitmapSource> GetAlbumCover()
        {
            BitmapSource bitmap = null;
            if (_bitmap.TryGetTarget(out bitmap))
                return bitmap;

            bitmap = new BitmapImage();
            bitmap.SetSource(await GetThumbnail());
            _bitmap.SetTarget(bitmap);

            return bitmap;
        }

        public async Task<MediaPlaybackItem> GetPlaybackItem()
        {
            if (_playbackItem is null)
            {
                _playbackItem = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await GetFile()));
                var properties = _playbackItem.GetDisplayProperties();
                properties.Thumbnail = RandomAccessStreamReference.CreateFromStream(await GetThumbnail());
                _playbackItem.ApplyDisplayProperties(properties);
            }

            return _playbackItem;
        }

        public static async Task<MusicFileDTO> CreateFromFile(StorageFile file)
        {
            var data = await Factory.FromStorageFile("$OUTSIDE$", file, String.Empty);
            return new MusicFileDTO(data, file);
        }
    }
}