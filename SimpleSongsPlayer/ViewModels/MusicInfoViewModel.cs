using System;
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
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.Views.Controllers;

namespace SimpleSongsPlayer.ViewModels
{
    public class MusicInfoViewModel : ViewModelBase
    {
        private readonly ObservableCollection<MusicFileDTO> musicFiles = MusicFileDataServer.Current.Data;
        private readonly ObservableCollection<LyricFileDTO> lyrics = LyricFileDataServer.Current.Data;

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
                await RefreshCover();
            }

            await LyricFileDataServer.Current.ScanFile();

            await RefreshLyricSource();

            CustomMediaPlayerElement.NowPlaybackItemChanged += CustomMediaPlayerElement_NowPlaybackItemChanged;
            LyricFileDataServer.Current.DataAdded += LyricDataDataServer_DataAdded;
        }

        private async Task RefreshLyricSource()
        {
            if (musicSource != null)
            {
                FlyoutNotification.Show(StringResources.NotificationStringResource.GetString("SearchLyricFile"));
                string musicName = musicSource.FileName.TrimExtensionName();
                var lyricDto = lyrics.FirstOrDefault(lf => lf.FileName.TrimExtensionName() == musicName);
                if (lyricDto != null)
                {
                    LyricSource = lyricDto;
                    await lyricDto.Init();
                }
                FlyoutNotification.Hide();
            }
        }

        private async Task RefreshMusicSource(MediaPlaybackItem playbackItem)
        {
            foreach (var fileDto in musicFiles.Where(mf => mf.IsInitPlaybackItem))
            {
                var item = await fileDto.GetPlaybackItem();
                if (playbackItem == item)
                {
                    MusicSource = fileDto;

                    break;
                }
            }
        }

        private async Task RefreshCover()
        {
            if (musicSource is null)
                return;

            Cover = await musicSource.GetAlbumCover();
        }

        private async void CustomMediaPlayerElement_NowPlaybackItemChanged(CustomMediaPlayerElement sender, PlayerNowPlaybackItemChangeEventArgs args)
        {
            if (args.NewItem != null)
            {
                await RefreshMusicSource(args.NewItem);
                await RefreshCover();
                await RefreshLyricSource();
            }
        }

        private async void LyricDataDataServer_DataAdded(object sender, IEnumerable<LyricFileDTO> e)
        {
            await RefreshLyricSource();
        }
    }
}