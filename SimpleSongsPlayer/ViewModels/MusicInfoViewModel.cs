using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
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
        private readonly ObservableCollection<MusicFileDTO> musicFiles = MusicLibraryDataServer.Current.MusicFilesList;
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
            LyricFileDataServer.Current.FilesAdded += LyricFileDataServer_FilesAdded;
        }

        private async Task RefreshLyricSource()
        {
            if (musicSource != null)
            {
                string musicName = musicSource.FileName.TrimExtensionName();
                var lyricDto = lyrics.FirstOrDefault(lf => lf.FileName.TrimExtensionName() == musicName);
                if (lyricDto != null)
                {
                    LyricSource = lyricDto;
                    await lyricDto.Init();
                }
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

        private async void LyricFileDataServer_FilesAdded(object sender, IEnumerable<LyricFileDTO> e)
        {
            await RefreshLyricSource();
        }
    }
}