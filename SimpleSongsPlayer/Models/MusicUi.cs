using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.Dal;

using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;

namespace SimpleSongsPlayer.Models
{
    public class MusicUi : ObservableObject
    {
        private MusicFile _table;
        private bool _isPlaying;

        public MusicUi(MusicFile table)
        {
            _table = table;

            Title = string.IsNullOrWhiteSpace(table.Title) ? Path.GetFileNameWithoutExtension(_table.FilePath) : table.Title;

            WeakReferenceMessenger.Default.Register<MusicUi, string, int>
                (this, Index, (mu, isPlaying) => mu.IsPlaying = bool.Parse(isPlaying));
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public int Index => _table.Index;
        public string Title { get; }
        public string Artist => _table.Artist;
        public string Album => _table.Album;
        public TimeSpan Duration => _table.Duration;
        public string FilePath => _table.FilePath;

        public bool IsInPlaybackList
        {
            get => _table.IsInPlaybackList;
            set => _table.IsInPlaybackList = value;
        }

        public MusicFile GetTable() => _table;

        public async Task<StorageFile> GetFileAsync()
        {
            return await StorageFile.GetFileFromPathAsync(_table.FilePath);
        }
    }
}
