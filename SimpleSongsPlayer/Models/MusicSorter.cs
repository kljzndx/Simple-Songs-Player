using System;
using Windows.ApplicationModel.Resources;
using Windows.Globalization.Collation;

namespace SimpleSongsPlayer.Models
{
    public class MusicSorter
    {
        private static readonly ResourceLoader ListStringResource = ResourceLoader.GetForCurrentView("MusicListPage");

        public MusicSorter(string resourceKey, Func<MusicFileDynamic, CharacterGroupings, IComparable> keySelector)
        {
            Name = ListStringResource.GetString(resourceKey);
            KeySelector = keySelector;
        }

        public string Name { get; }
        public Func<MusicFileDynamic, CharacterGroupings, IComparable> KeySelector { get; }
    }
}