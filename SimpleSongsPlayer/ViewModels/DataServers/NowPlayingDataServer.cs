using System;
using System.Collections.Generic;
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
    public class NowPlayingDataServer : IFileDataServer<MusicFileDTO>
    {
        public static  readonly NowPlayingDataServer Current = new NowPlayingDataServer();

        private MediaPlaybackList currentPlaybackList;

        private NowPlayingDataServer()
        {
        }

        public bool IsInit { get; } = true;
        public ObservableCollection<MusicFileDTO> Data { get; } = new ObservableCollection<MusicFileDTO>();
        public event EventHandler<IEnumerable<MusicFileDTO>> DataAdded;
        public event EventHandler<IEnumerable<MusicFileDTO>> DataRemoved;

        public async Task Initialize()
        {
            if (App.MediaPlayer.Source is MediaPlaybackList mpl && currentPlaybackList != mpl)
                await SetUpSource(mpl);
        }

        public async Task SetUpSource(MediaPlaybackList playbackList)
        {
            Data.Clear();
            if (currentPlaybackList != null)
                currentPlaybackList.Items.VectorChanged -= PlaybackItems_VectorChanged;
            foreach (var playbackItem in playbackList.Items)
            {
                var dto = await GetFile(playbackItem);

                if (dto != null)
                    Data.Add(dto);
                else if (MusicFileDataServer.Current.Data.Any())
                    new Exception("音乐库里找不到该歌曲").ShowErrorDialog();
            }

            DataAdded?.Invoke(this, Data);
            currentPlaybackList = playbackList;
            currentPlaybackList.Items.VectorChanged += PlaybackItems_VectorChanged;
        }

        private async Task<MusicFileDTO> GetFile(MediaPlaybackItem playbackItem)
        {
            MusicFileDTO dto = null;
            foreach (var currentDto in MusicFileDataServer.Current.Data.Where(d => d.IsInitPlaybackItem))
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
                {
                    var dto = await GetFile(sender[(int)args.Index]);
                    Data.Insert((int)args.Index, dto);
                    DataAdded?.Invoke(this, new[] {dto});
                    break;
                }
                case CollectionChange.ItemRemoved:
                {
                    var dto = await GetFile(sender[(int)args.Index]);
                    Data.Remove(dto);
                    DataRemoved?.Invoke(this, new[] {dto});
                    break;
                }
            }
        }
    }
}