using System.Collections.Generic;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models.DTO;

namespace SimpleSongsPlayer.Models
{
    public class MusicFileGroup : ObservableObject
    {
        private string name;

        public MusicFileGroup(string name, IList<MusicFileDTO> items)
        {
            this.name = name;
            Items = items;
        }

        public MusicFileGroup(string name, IList<MusicFileDTO> items, BitmapSource cover) : this(name, items)
        {
            Cover = cover;
        }

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        public BitmapSource Cover { get; }
        public IList<MusicFileDTO> Items { get; }
    }
}