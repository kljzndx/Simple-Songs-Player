using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Views;

namespace SimpleSongsPlayer.Models
{
    public class MusicFileGroup : ObservableObject
    {
        private string name;

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items)
        {
            this.name = name;
            Items = new ObservableCollection<MusicFileDTO>(items);
        }

        public MusicFileGroup(string name, IEnumerable<MusicFileDTO> items, BitmapSource cover) : this(name, items)
        {
            Cover = cover;
        }

        public string Name
        {
            get => name;
            set
            {
                string str = name;
                Set(ref name, value);
                Renamed?.Invoke(this, new KeyValuePair<string, string>(str, value));
            }
        }

        public BitmapSource Cover { get; }
        public ObservableCollection<MusicFileDTO> Items { get; }

        public event TypedEventHandler<MusicFileGroup, KeyValuePair<string, string>> Renamed;
    }
}