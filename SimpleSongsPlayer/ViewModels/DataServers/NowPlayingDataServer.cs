using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Extensions;

namespace SimpleSongsPlayer.ViewModels
{
    public class NowPlayingDataServer
    {
        public static  readonly NowPlayingDataServer Current = new NowPlayingDataServer();

        private MediaPlaybackList currentPlaybackList;

        private NowPlayingDataServer()
        {
        }

        public ObservableCollection<MusicFileDTO> DataSource { get; } = new ObservableCollection<MusicFileDTO>();

        public async Task Initialize()
        {
            if (App.MediaPlayer.Source is MediaPlaybackList mpl && currentPlaybackList != mpl)
                await SetUpSource(mpl);
        }

        public async Task SetUpSource(MediaPlaybackList playbackList)
        {
            DataSource.Clear();
            if (currentPlaybackList != null)
                currentPlaybackList.Items.VectorChanged -= PlaybackItems_VectorChanged;
            foreach (var playbackItem in playbackList.Items)
            {
                var dto = await GetFile(playbackItem);

                if (dto != null)
                    DataSource.Add(dto);
                else if (MusicLibraryDataServer.Current.MusicFilesList.Any())
                    new Exception("音乐库里找不到该歌曲").ShowErrorDialog();
            }

            currentPlaybackList = playbackList;
            currentPlaybackList.Items.VectorChanged += PlaybackItems_VectorChanged;
        }

        private async Task<MusicFileDTO> GetFile(MediaPlaybackItem playbackItem)
        {
            MusicFileDTO dto = null;
            foreach (var currentDto in MusicLibraryDataServer.Current.MusicFilesList.Where(d => d.IsInitPlaybackItem))
            {
                var p = await currentDto.GetPlaybackItem();
                if (p == playbackItem)
                {
                    dto = currentDto;
                    break;
                }
            }

            return dto;
        }
        
        private async void PlaybackItems_VectorChanged(IObservableVector<MediaPlaybackItem> sender, IVectorChangedEventArgs args)
        {
            switch (args.CollectionChange)
            {
                case CollectionChange.ItemInserted:
                    DataSource.Insert((int)args.Index, await GetFile(sender[(int)args.Index]));
                    break;
                case CollectionChange.ItemRemoved:
                    DataSource.RemoveAt((int)args.Index);
                    break;
            }
        }
    }
}