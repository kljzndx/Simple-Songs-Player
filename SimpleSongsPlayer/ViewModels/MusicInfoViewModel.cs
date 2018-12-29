﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.Views.Controllers;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicInfoViewModel : ViewModelBase
    {
        private readonly ObservableCollection<MusicFileDTO> musicFiles = MusicFileDataServer.Current.Data;
        private readonly ObservableCollection<KeyValuePair<MusicFileDTO, LyricFileDTO>> indexes =
            LyricIndexDataServer.Current.Data;

        private MusicFileDTO musicSource;
        private LyricFileDTO lyricSource;
        private BitmapSource cover;

        public MusicInfoViewModel()
        {
        }
        
        public MusicFileDTO MusicSource
        {
            get => musicSource;
            set => Set(ref musicSource, value);
        }

        public LyricFileDTO LyricSource
        {
            get => lyricSource;
            set => Set(ref lyricSource, value);
        }

        public BitmapSource Cover
        {
            get => cover;
            set => Set(ref cover, value);
        }

        public async Task Init()
        {
            if (musicSource != null)
                return;

            var currentItem = CustomMediaPlayerElement.CurrentItem;
            if (currentItem != null)
            {
                await RefreshMusicSource(currentItem);
                await RefreshLyricSource();
            }

            this.LogByObject("监听播放曲目更改事件");
            CustomMediaPlayerElement.NowPlaybackItemChanged += CustomMediaPlayerElement_NowPlaybackItemChanged;
        }

        private async Task RefreshLyricSource()
        {
            FlyoutNotification.Show(StringResources.NotificationStringResource.GetString("SearchLyricFile"));

            var pair = indexes.FirstOrDefault(i => i.Key.FilePath == musicSource.FilePath);
            if (pair.Value != null)
            {
                this.LogByObject("正在解析歌词");
                LyricSource = pair.Value;
                await pair.Value.Init();
            }

            FlyoutNotification.Hide();
        }

        private async Task RefreshMusicSource(MediaPlaybackItem playbackItem)
        {
            foreach (var fileDto in musicFiles.Where(mf => mf.IsInitPlaybackItem))
            {
                var item = await fileDto.GetPlaybackItem();
                if (playbackItem == item)
                {
                    this.LogByObject("正在刷新音乐信息");
                    MusicSource = fileDto;
                    Cover = await fileDto.GetAlbumCover();
                    break;
                }
            }
        }
        
        private async void CustomMediaPlayerElement_NowPlaybackItemChanged(CustomMediaPlayerElement sender, PlayerNowPlaybackItemChangeEventArgs args)
        {
            if (args.NewItem != null)
            {
                await RefreshMusicSource(args.NewItem);
                await RefreshLyricSource();
            }
        }
    }
}