using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.SettingProperties;

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
            _playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
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

                var id = Data.Count;
                Data.Add(dto);

                if (id == OtherSettingProperties.Current.CurrentPlayIndex)
                    _playbackList.MoveTo((uint) id);
            }

            if (OtherSettingProperties.Current.CurrentPlayIndex >= Data.Count)
                OtherSettingProperties.Current.CurrentPlayIndex = 0;

            if (Data.Any())
                DataAdded?.Invoke(this, Data);

            IsInit = true;
        }

        public async Task Push(MusicFileDTO music)
        {
            int id = 0;
            try
            {
                var dto = Data.FirstOrDefault(f => f.FilePath == music.FilePath) ?? music;

                var item = await dto.GetPlaybackItem();
                if (!_playbackList.Items.Contains(item))
                {
                    this.LogByObject("将播放项插入至顶端");
                    _playbackList.Items.Insert(0, item);

                    if (Data.Contains(dto))
                        Data.Remove(dto);

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
            
            var item = await music.GetPlaybackItem();
            uint id = _playbackList.CurrentItemIndex + 1;
            bool isAdd = id == _playbackList.Items.Count;

            try
            {
                if (_playbackList.CurrentItem == item)
                    return;

                if (_playbackList.Items.Contains(item))
                {
                    this.LogByObject("移除播放项");
                    if (_playbackList.Items.IndexOf(item) <= id)
                        id--;

                    _playbackList.Items.Remove(item);
                }

                this.LogByObject("将播放项插入至当前项下方");
                if (isAdd)
                    _playbackList.Items.Add(item);
                else
                    _playbackList.Items.Insert((int) id, item);

                if (_playbackList.ShuffleEnabled)
                {
                    var shuffleList = _playbackList.ShuffledItems.ToList();
                    if (shuffleList.Any(item.Equals))
                    {
                        shuffleList.Remove(item);

                        var cid = shuffleList.IndexOf(_playbackList.CurrentItem);
                        shuffleList.Insert(cid + 1, item);

                        _playbackList.SetShuffledItems(shuffleList);
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception($"Cannot Parse {music.FileName} file", e);
            }

            if (Data.Contains(music))
                Data.Remove(music);

            if (isAdd)
            {
                Data.Add(music);
                await _service.Add(music.FilePath);
            }
            else
            {
                Data.Insert((int)id, music);
                await _service.SetUp(Data.Select(d => d.FilePath));
            }

            DataAdded?.Invoke(this, new[] { music });
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

                if (Data.Contains(dto))
                    Data.Remove(dto);

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
            foreach (var item in data.ToList())
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
                            var file = await StorageFile.GetFileFromPathAsync(item.Path);
                            var music = await MusicFileDTO.CreateFromFile(file);
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

        private void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            if (IsInit)
                OtherSettingProperties.Current.CurrentPlayIndex = sender.CurrentItemIndex;
        }

        private void Data_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Data.Any())
            {
                if (App.MediaPlayer.Source is null && _playbackList.CurrentItemIndex >= OtherSettingProperties.Current.CurrentPlayIndex)
                    App.MediaPlayer.Source = _playbackList;

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Remove:
                        foreach (MusicFileDTO item in e.OldItems)
                            item.IsPlaying = false;
                        break;
                }
            }
            else
                App.MediaPlayer.Source = null;
        }
    }
}