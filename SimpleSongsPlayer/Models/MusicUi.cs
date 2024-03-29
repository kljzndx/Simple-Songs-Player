﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.Dal;
using SimpleSongsPlayer.Services;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace SimpleSongsPlayer.Models
{
    public class MusicUi : ObservableObject
    {
        private MusicFile _table;
        private bool _isPlaying;

        public MusicUi(MusicFile table)
        {
            _table = table;

            IsPlaying = Id == GetCurrentPlayId();
            Title = string.IsNullOrWhiteSpace(table.Title) ? Path.GetFileNameWithoutExtension(_table.FilePath) : table.Title;

            WeakReferenceMessenger.Default.Register<MusicUi, string, string>(this, nameof(PlaybackListManageService), (mu, mes) => 
            {
                if (mes == "CurrentPlayChanged")
                    mu.IsPlaying = mu.Id == mu.GetCurrentPlayId();
            });
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public int Id => _table.MusicFileId;
        public string Title { get; }
        public string Artist => _table.Artist;
        public string Album => _table.Album;
        public uint TrackNumber => _table.TrackNumber;
        public TimeSpan Duration => _table.Duration;

        public string FilePath => _table.FilePath;
        public DateTime ModifyDate => _table.FileChangeDate;

        private int GetCurrentPlayId()
            => Ioc.Default.GetRequiredService<PlaybackListManageService>().CurrentPlayItem?.MusicFileId ?? -1;

        public MusicFile GetTable() => _table;

        public async Task<StorageFile> GetFileAsync()
        {
            return await StorageFile.GetFileFromPathAsync(_table.FilePath);
        }

        public async Task<BitmapSource> GetCoverAsync()
        {
            var image = new BitmapImage();
            await image.SetSourceAsync(await GetThumbnailAsync());
            return image;
        }

        public async Task<MediaPlaybackItem> GetPlaybackItem()
        {
            var item = new MediaPlaybackItem(MediaSource.CreateFromStorageFile(await GetFileAsync()));
            var prop = item.GetDisplayProperties();
            prop.Thumbnail = RandomAccessStreamReference.CreateFromStream(await GetThumbnailAsync());
            item.ApplyDisplayProperties(prop);

            return item;
        }

        private async Task<StorageItemThumbnail> GetThumbnailAsync()
        {
            return await (await GetFileAsync()).GetThumbnailAsync(ThumbnailMode.SingleItem);
        }
    }
}
