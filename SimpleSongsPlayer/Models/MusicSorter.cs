using System;
using Windows.ApplicationModel.Resources;
using Windows.Globalization.Collation;

namespace SimpleSongsPlayer.Models
{
    public delegate IComparable MusicDynamicSortKeySelector(MusicFileDynamic fileDynamic, CharacterGroupings cgs);

    public class MusicSorter
    {
        private static readonly ResourceLoader ListStringResource = ResourceLoader.GetForCurrentView("MusicListPage");

        public MusicSorter(string resourceKey, MusicDynamicSortKeySelector keySelector)
        {
            Name = ListStringResource.GetString(resourceKey);
            KeySelector = keySelector;
        }

        public string Name { get; }
        public MusicDynamicSortKeySelector KeySelector { get; }
    }
}