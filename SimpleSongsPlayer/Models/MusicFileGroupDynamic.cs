using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.Globalization.Collation;
using GalaSoft.MvvmLight;

namespace SimpleSongsPlayer.Models
{
    public class MusicFileGroupDynamic : ObservableObject
    {
        public MusicFileGroupDynamic(MusicFileGroup group)
        {
            Name = group.Name;
            Items = new ObservableCollection<MusicFileDynamic>();
            foreach (var item in group.Items)
                Items.Add(new MusicFileDynamic(item));
        }

        public string Name { get; }
        public ObservableCollection<MusicFileDynamic> Items { get; }

        public void Join(MusicFileGroup data)
        {
            foreach (var dto in data.Items)
                Items.Add(new MusicFileDynamic(dto));
        }

        public void OrderBy(MusicDynamicSortKeySelector<MusicFileDynamic> keySelector)
        {
            var newItems = Items.OrderBy(keySelector.Invoke).ToList();
            foreach (var item in newItems)
            {
                var oldId = Items.IndexOf(item);
                var newId = newItems.IndexOf(item);
                Items.Move(oldId, newId);
            }
        }

        public void ReverseItems()
        {
            var newItems = Items.Reverse().ToList();
            foreach (var item in newItems)
            {
                var oldId = Items.IndexOf(item);
                var newId = newItems.IndexOf(item);
                Items.Move(oldId, newId);
            }
        }
    }
}