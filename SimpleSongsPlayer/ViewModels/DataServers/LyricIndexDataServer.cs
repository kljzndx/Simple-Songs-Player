using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public class LyricIndexDataServer : IFileDataServer<KeyValuePair<MusicFileDTO, LyricFileDTO>>
    {
        public static LyricIndexDataServer Current = new LyricIndexDataServer();

        private readonly ObservableCollection<MusicFileDTO> _musicFiles = MusicFileDataServer.Current.Data;
        private readonly ObservableCollection<LyricFileDTO> _lyricFiles = LyricFileDataServer.Current.Data;

        private LyricIndexService _service;

        private LyricIndexDataServer()
        {
        }

        public bool IsInit { get; private set; }
        public ObservableCollection<KeyValuePair<MusicFileDTO, LyricFileDTO>> Data { get; } = new ObservableCollection<KeyValuePair<MusicFileDTO, LyricFileDTO>>();
        public event EventHandler<IEnumerable<KeyValuePair<MusicFileDTO, LyricFileDTO>>> DataAdded;
        public event EventHandler<IEnumerable<KeyValuePair<MusicFileDTO, LyricFileDTO>>> DataRemoved;
        public event EventHandler<IEnumerable<KeyValuePair<MusicFileDTO, LyricFileDTO>>> DataUpdated;

        public async Task Init()
        {
            if (IsInit)
                return;

            this.LogByObject("初始化索引服务");
            IsInit = true;
            _service = await LyricIndexService.GetService();

            if (!_musicFiles.Any())
                await MusicFileDataServer.Current.InitializeMusicService();
            if (!_lyricFiles.Any())
                await LyricFileDataServer.Current.Init();
            if (!_musicFiles.Any() || !_lyricFiles.Any())
                return;

            var results = await _service.GetData();
            if (results.Any())
            {
                this.LogByObject("正在解析索引数据");
                foreach (var lyricIndex in results)
                {
                    var pair = GetPair(lyricIndex);
                    if (pair.HasValue)
                        Data.Add(pair.Value);
                }

                this.LogByObject("触发数据添加事件");
                DataAdded?.Invoke(this, Data.ToList());
            }

            this.LogByObject("监听服务");
            _service.DataAdded += Service_DataAdded;
            _service.DataRemoved += Service_DataRemoved;
            _service.DataUpdated += Service_DataUpdated;
        }

        public async Task ScanAsync()
        {
            if (!IsInit)
                await Init();

            await _service.ScanAsync();
        }

        public Task SetIndex(string musicPath, string lyricPath) => _service.SetIndex(musicPath, lyricPath);
        
        private IEnumerable<KeyValuePair<MusicFileDTO, LyricFileDTO>> IntelligentOption(IEnumerable<LyricIndex> source, Action<KeyValuePair<MusicFileDTO, LyricFileDTO>> action)
        {
            foreach (var lyricIndex in source)
            {
                var pair = GetPair(lyricIndex);
                if (pair.HasValue)
                {
                    action.Invoke(pair.Value);
                    yield return pair.Value;
                }
            }
        }
        
        private Nullable<KeyValuePair<MusicFileDTO, LyricFileDTO>> GetPair(LyricIndex lyricIndex)
        {
            var musicFile = _musicFiles.FirstOrDefault(m => m.FilePath == lyricIndex.MusicPath);
            var lyricFile = _lyricFiles.FirstOrDefault(l => l.FilePath == lyricIndex.LyricPath);
            if (musicFile == null || lyricFile == null)
                return null;

            var result = new KeyValuePair<MusicFileDTO, LyricFileDTO>(musicFile, lyricFile);
            return result;
        }
        
        private void Service_DataAdded(object sender, IEnumerable<LyricIndex> e)
        {
            var pairs = IntelligentOption(e, Data.Add).ToList();
            if (pairs.Any())
            {
                this.LogByObject("检测到有新的索引项，已成功同步");
                DataAdded?.Invoke(this, pairs);
            }
        }

        private void Service_DataRemoved(object sender, IEnumerable<LyricIndex> e)
        {
            var pairs = IntelligentOption(e, p =>
            {
                if (!p.Value.IsInit)
                    Data.Remove(p);
            }).ToList();
            if (pairs.Any())
            {
                this.LogByObject("检测到有索引项被移除，已成功同步");
                DataRemoved?.Invoke(this, pairs);
            }
        }

        private void Service_DataUpdated(object sender, IEnumerable<LyricIndex> e)
        {
            var pairs = IntelligentOption(e, p =>
            {
                try
                {
                    var d = Data.First(op => op.Key.FilePath == p.Key.FilePath);
                    var id = Data.IndexOf(d);

                    Data.Remove(d);
                    Data.Insert(id, d);
                }
                catch (Exception)
                {
                }
            }).ToList();

            if (pairs.Any())
            {
                this.LogByObject("检测到有索引项被更新，已成功同步");
                DataUpdated?.Invoke(this, pairs);
            }
        }
    }
}