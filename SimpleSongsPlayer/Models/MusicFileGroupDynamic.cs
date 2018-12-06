using System.Collections.ObjectModel;
using System.ComponentModel;
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
    }
}