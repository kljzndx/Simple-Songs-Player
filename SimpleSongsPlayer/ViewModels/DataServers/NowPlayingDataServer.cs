using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
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
            MusicFileDataServer.Current.DataRemoved += MusicFileDataServer_DataRemoved;
        }

        public bool IsInit { get; } = true;
        public ObservableCollection<MusicFileDTO> Data { get; } = new ObservableCollection<MusicFileDTO>();
        public event EventHandler<IEnumerable<MusicFileDTO>> DataAdded;
        public event EventHandler<IEnumerable<MusicFileDTO>> DataRemoved;

        public async Task SetUpSource(MediaPlaybackList playbackList)
        {
            this.LogByObject("初始化列表");
            Data.Clear();
            if (currentPlaybackList != null)
            {
                this.LogByObject("取消对旧播放源的监听");
                currentPlaybackList.Items.VectorChanged -= PlaybackItems_VectorChanged;
            }

            if (playbackList.Items.Any())
            {
                this.LogByObject("正在解析播放源");
                foreach (var playbackItem in playbackList.Items)
                {
                    var dto = await GetFile(playbackItem);

                    if (dto != null)
                        Data.Add(dto);
                    else if (MusicFileDataServer.Current.Data.Any())
                        new Exception("音乐库里找不到该歌曲").ShowErrorDialog();
                }
            }

            this.LogByObject("监听播放源");
            currentPlaybackList = playbackList;
            currentPlaybackList.Items.VectorChanged += PlaybackItems_VectorChanged;

            this.LogByObject("触发数据添加事件");
            DataAdded?.Invoke(this, Data);
        }

        private async Task<MusicFileDTO> GetFile(MediaPlaybackItem playbackItem)
        {
            MusicFileDTO dto = null;
            foreach (var currentDto in MusicFileDataServer.Current.Data.Where(d => d.IsInitPlaybackItem))
            {
                var p = await currentDto.GetPlaybackItem();
                if (p == playbackItem)
                {
                    this.LogByObject("已找到对应音乐文件");
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
                    var dto = await GetFile(sender[(int) args.Index]);
                    if (dto != null)
                    {
                        this.LogByObject("正在添加播放项");
                        Data.Insert((int)args.Index, dto);
                        DataAdded?.Invoke(this, new[] {dto});
                    }
                    break;
                }
                case CollectionChange.ItemRemoved:
                {
                    var dto = Data[(int) args.Index];
                    if (dto != null)
                    {
                        this.LogByObject("正在移除播放项");
                        Data.Remove(dto);
                        DataRemoved?.Invoke(this, new[] {dto});
                    }

                    if (!sender.Any())
                    {
                        App.MediaPlayer.Source = null;
                        currentPlaybackList.Items.VectorChanged -= PlaybackItems_VectorChanged;
                        currentPlaybackList = null;
                    }
                    break;
                }
            }
        }

        private async void MusicFileDataServer_DataRemoved(object sender, IEnumerable<MusicFileDTO> e)
        {
            List<MusicFileDTO> optionList = new List<MusicFileDTO>();

            foreach (var musicFileDto in e)
                if (Data.Contains(musicFileDto))
                {
                    currentPlaybackList?.Items.Remove(await musicFileDto.GetPlaybackItem());
                    optionList.Add(musicFileDto);
                }

            if (optionList.Any())
            {
                this.LogByObject("检测到有文件被删除，已经同步完成");
                DataRemoved?.Invoke(this, optionList);
            }
        }
    }
}