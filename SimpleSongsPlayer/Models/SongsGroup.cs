using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml.Media.Imaging;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models
{
    public class SongsGroup
    {
        public SongsGroup(string name)
        {
            Name = name;
            Items = new ObservableCollection<Song>();
        }

        public SongsGroup(string name, IEnumerable<Song> items)
        {
            Name = name;
            Items = new ObservableCollection<Song>(items);

            var song = Items.LastOrDefault();
            if (song != null)
                AlbumCover = song.AlbumCover;
        }

        public bool IsAny => Items.Any();
        public string Name { get; }
        public BitmapSource AlbumCover { get; set; }
        public ObservableCollection<Song> Items { get; }
    }
}