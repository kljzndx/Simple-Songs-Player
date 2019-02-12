using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public class PlaybackListDataServer : IFileDataServer<MusicFileDTO>
    {
        public static readonly PlaybackListDataServer Current = new PlaybackListDataServer();

        private readonly MusicFileDataServer _musicFileDataServer = MusicFileDataServer.Current;
        private PlaybackListService _service;
        private readonly MediaPlaybackList _playbackList = new MediaPlaybackList();

        private PlaybackListDataServer()
        {
            Data.CollectionChanged += Data_CollectionChanged;
        }

        private void Data_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Data.Any())
            {
                if (App.MediaPlayer.Source is null)
                    App.MediaPlayer.Source = _playbackList;
            }
            else
                App.MediaPlayer.Source = null;

        }

        public bool IsInit { get; private set; }
        public ObservableCollection<MusicFileDTO> Data { get; } = new ObservableCollection<MusicFileDTO>();
        public event EventHandler<IEnumerable<MusicFileDTO>> DataAdded;
        public event EventHandler<IEnumerable<MusicFileDTO>> DataRemoved;

        public async Task Init()
        {
            if (IsInit)
                return;

            if (!_musicFileDataServer.IsInit)
                await _musicFileDataServer.InitializeMusicService();

            IsInit = true;
            _service = await PlaybackListService.GetService();
            var data = await _service.GetData();

            foreach (var dto in await Parse(data))
            {
                try
                {
                    var item = await dto.GetPlaybackItem();
                    AddToPlaybackList(item);
                }
                catch (Exception e)
                {
                    this.LogByObject(e, $"{dto.FileName} 获取播放项失败");
                    await _service.Remove(dto.FilePath);
                    continue;
                }

                Data.Add(dto);
            }

            if (Data.Any())
                DataAdded?.Invoke(this, Data);
        }

        public async Task Push(MusicFileDTO music)
        {
            int id = 0;
            try
            {
                var item = await music.GetPlaybackItem();
                if (!_playbackList.Items.Contains(item) && _playbackList.CurrentItem != item)
                {
                    this.LogByObject("将播放项插入至顶端");
                    _playbackList.Items.Insert(0, item);
                    Data.Insert(0, music);
                }

                id = _playbackList.Items.IndexOf(item);
            }
            catch (Exception e)
            {
                throw new Exception($"Cannot Parse {music.FileName} file", e);
            }

            _playbackList.MoveTo((uint) id);
            Play();

            if (id == 0)
            {
                await _service.SetUp(Data.Select(d => d.FilePath));
                DataAdded?.Invoke(this, new[] {music});
            }
        }

        public async Task PushToNext(MusicFileDTO music)
        {
            if (!Data.Any())
            {
                await SetUp(new[] {music});
                return;
            }

            uint id = _playbackList.CurrentItemIndex + 1;
            bool isAdd = true;
            try
            {
                var item = await music.GetPlaybackItem();
                if (_playbackList.CurrentItem == item)
                {
                    isAdd = false;
                    return;
                }

                if (_playbackList.Items.Contains(item))
                {
                    isAdd = false;
                    _playbackList.Items.Remove(item);
                }

                this.LogByObject("将播放项插入至顶端");
                _playbackList.Items.Insert((int) id, item);
            }
            catch (Exception e)
            {
                throw new Exception($"Cannot Parse {music.FileName} file", e);
            }

            Data.Insert((int)id, music);
            if (isAdd)
            {
                await _service.SetUp(Data.Select(d => d.FilePath));
                DataAdded?.Invoke(this, new[] { music });
            }
        }

        public async Task Append(IEnumerable<MusicFileDTO> musicList)
        {
            var source = musicList.ToList();

            if (!Data.Any())
            {
                await SetUp(source);
                return;
            }

            var result = await _service.AddRange(source.Select(f => f.FilePath));
            foreach (var dto in source.Where(mf => result.Any(p => p.Path == mf.FilePath)))
            {
                try
                {
                    var item = await dto.GetPlaybackItem();
                    AddToPlaybackList(item);
                }
                catch (Exception e)
                {
                    this.LogByObject(e, $"{dto.FileName} 获取播放项失败");
                    await _service.Remove(dto.FilePath);
                    continue;
                }

                Data.Add(dto);
            }

            DataAdded?.Invoke(this, source);
        }

        public async Task Remove(MusicFileDTO music)
        {
            var item = await music.GetPlaybackItem();
            if (_playbackList.Items.Contains(item))
                _playbackList.Items.Remove(item);
            if (Data.Contains(music))
                Data.Remove(music);
        }

        public async Task SetUp(IEnumerable<MusicFileDTO> musicList)
        {
            foreach (var musicFileDto in Data.ToList())
                Data.Remove(musicFileDto);
            _playbackList.Items.Clear();

            var source = musicList.ToList();
            var result = await _service.SetUp(source.Select(f => f.FilePath));
            foreach (var dto in source.Where(mf => result.Any(p => p.Path == mf.FilePath)))
            {
                try
                {
                    var item = await dto.GetPlaybackItem();
                    AddToPlaybackList(item);
                }
                catch (Exception e)
                {
                    this.LogByObject(e, $"{dto.FileName} 获取播放项失败");
                    await _service.Remove(dto.FilePath);
                    continue;
                }

                Data.Add(dto);
            }

            Play();

            DataAdded?.Invoke(this, source);
        }

        private void Play()
        {
            if (App.MediaPlayer.PlaybackSession is null || App.MediaPlayer.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                App.MediaPlayer.Play();
        }

        private async Task<List<MusicFileDTO>> Parse(IEnumerable<PlaybackItem> data)
        {
            var result = new List<MusicFileDTO>();
            foreach (var item in data)
            {
                switch (item.Source)
                {
                    case FileSourceMembers.MusicLibrary:
                    {
                        var music = _musicFileDataServer.Data.FirstOrDefault(mf => mf.FilePath == item.Path);
                        if (music != null)
                            result.Add(music);
                        else
                            await _service.Remove(item.Path);
                        break;
                    }
                    case FileSourceMembers.Other:
                    {
                        try
                        {
                            var music = await MusicFileDTO.CreateFromPath(item.Path);
                            result.Add(music);
                        }
                        catch (Exception e)
                        {
                            await _service.Remove(item.Path);
                        }

                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return result;
        }

        private void AddToPlaybackList(MediaPlaybackItem item)
        {
            if (!_playbackList.Items.Contains(item))
                _playbackList.Items.Add(item);
        }

        //private async void Service_DataAdded(object sender, IEnumerable<PlaybackItem> e)
        //{
        //    var needAdd = await Parse(e);
        //    foreach (var dto in needAdd)
        //        Data.Add(dto);

        //    DataAdded?.Invoke(this, needAdd);
        //}

        //private void Service_DataRemoved(object sender, IEnumerable<PlaybackItem> e)
        //{
        //    var needRemove = Data.Where(mf => e.Any(i => i.Path == mf.FilePath)).ToList();
        //    foreach (var dto in needRemove)
        //        Data.Remove(dto);

        //    DataRemoved?.Invoke(this, needRemove);
        //}
    }
}