using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
            Items = new ObservableCollection<SongItem>();
            Items.CollectionChanged += Items_CollectionChanged;
        }

        public SongsGroup(string name, IEnumerable<SongItem> items)
        {
            _name = name;
            Items = new ObservableCollection<SongItem>(items);
            Items.CollectionChanged += Items_CollectionChanged;

            foreach (var songItem in Items)
            {
                songItem.RemoveRequested -= SongItem_RemoveRequested;
                songItem.RemoveRequested += SongItem_RemoveRequested;
            }

            var song = Items.LastOrDefault();
            if (song != null)
                AlbumCover = song.AlbumCover;
        }

        private void Items_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SongItem item in e.NewItems)
                    {
                        item.RemoveRequested -= SongItem_RemoveRequested;
                        item.RemoveRequested += SongItem_RemoveRequested;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (SongItem item in e.OldItems)
                        item.RemoveRequested -= SongItem_RemoveRequested;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SongItem_RemoveRequested(SongItem sender, System.EventArgs args)
        {
            Items.Remove(sender);
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
        public ObservableCollection<SongItem> Items { get; }

        public event TypedEventHandler<SongsGroup, SongGroupRenamedEventArgs> Renamed;
    }
}