using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
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
        private event EventHandler<LyricIndex> _queryFailed;

        private LyricIndexDataServer()
        {
            _queryFailed += LyricIndexDataServer_QueryFailed;
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

            IsInit = true;
            _service = await LyricIndexService.GetService();

            if (!_musicFiles.Any())
                await MusicFileDataServer.Current.InitializeMusicService();
            if (!_lyricFiles.Any())
                await LyricFileDataServer.Current.Init();
            if (!_musicFiles.Any() || !_lyricFiles.Any())
                return;

            var results = await _service.GetFiles();
            foreach (var lyricIndex in results)
            {
                var pair = GetPair(lyricIndex);
                if (pair.HasValue)
                    Data.Add(pair.Value);
                else
                    await RemoveIndex(lyricIndex.MusicPath);
            }

            DataAdded?.Invoke(this, Data.ToList());

            _service.FilesAdded += Service_FilesAdded;
            _service.FilesRemoved += Service_FilesRemoved;
            _service.FilesUpdated += Service_FilesUpdated;
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
                else
                    _queryFailed?.Invoke(this, lyricIndex);
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

        private async void LyricIndexDataServer_QueryFailed(object sender, LyricIndex e)
        {
            await RemoveIndex(e.MusicPath);
        }

        private void Service_FilesAdded(object sender, IEnumerable<LyricIndex> e)
        {
            var pairs = IntelligentOption(e, Data.Add);
            DataAdded?.Invoke(this, pairs);
        }

        private void Service_FilesRemoved(object sender, IEnumerable<LyricIndex> e)
        {
            var pairs = IntelligentOption(e, p => Data.Remove(p));
            DataRemoved?.Invoke(this, pairs);
        }

        private void Service_FilesUpdated(object sender, IEnumerable<LyricIndex> e)
        {
            var pairs = IntelligentOption(e, p =>
            {
                var id = Data.IndexOf(Data.First(op => op.Key.FilePath == p.Key.FilePath));
                Data[id] = p;
            });
            DataUpdated?.Invoke(this, pairs);
        }
    }
}