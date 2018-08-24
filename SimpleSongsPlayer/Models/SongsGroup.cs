using System.Collections.Generic;
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
            Items = new List<Song>();
        }

        public SongsGroup(string name, List<Song> items)
        {
            Name = name;
            Items = items;

            var song = items.FirstOrDefault();
            if (song != null)
                AlbumCover = song.AlbumCover;
        }

        public string Name { get; }
        public BitmapSource AlbumCover { get; set; }
        public List<Song> Items { get; }
    }
}