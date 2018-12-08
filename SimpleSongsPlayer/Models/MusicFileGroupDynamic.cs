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
        private static readonly CharacterGroupings CharacterGroupings = new CharacterGroupings();

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

        public void OrderBy(MusicDynamicSortKeySelector keySelector)
        {
            for (var i = Items.Count - 1; i >= 0; i--)
                for (int j = 0; j < i; j++)
                    if (keySelector(Items[j], CharacterGroupings).CompareTo(keySelector(Items[j + 1], CharacterGroupings)) > 0)
                        Items.Move(j, j + 1);
        }

        public void ReverseItems()
        {
            for (int i = 0; i < Items.Count; i++)
                Items.Move(0, Items.Count - i - 1);
        }
    }
}