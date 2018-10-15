using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models.Events;
using ObservableObject = GalaSoft.MvvmLight.ObservableObject;

namespace SimpleSongsPlayer.Models
{
    public class SongsGroup : ObservableObject
    {
        private string _name;

        public SongsGroup(string name)
        {
            _name = name;
            Items = new ObservableCollection<Song>();
        }

        public SongsGroup(string name, IEnumerable<Song> items)
        {
            _name = name;
            Items = new ObservableCollection<Song>(items);

            var song = Items.LastOrDefault();
            if (song != null)
                AlbumCover = song.AlbumCover;
        }

        public bool IsAny => Items.Any();

        public string Name
        {
            get => _name;
            set
            {
                string oldName = _name;

                Set(ref _name, value);

                Renamed?.Invoke(this, new SongGroupRenamedEventArgs(oldName, value));
            }
        }

        public BitmapSource AlbumCover { get; set; }
        public ObservableCollection<Song> Items { get; }

        public event TypedEventHandler<SongsGroup, SongGroupRenamedEventArgs> Renamed;
    }
}