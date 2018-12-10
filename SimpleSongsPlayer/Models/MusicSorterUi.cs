using System;
using Windows.ApplicationModel.Resources;
using Windows.Globalization.Collation;

namespace SimpleSongsPlayer.Models
{
    public delegate IComparable MusicDynamicSortKeySelector(MusicFileDynamic fileDynamic, CharacterGroupings cgs);

    public class MusicSorterUi
    {
        public MusicSorterUi(string resourceKey, MusicDynamicSortKeySelector keySelector, bool isReverse = false)
        {
            Name = StringResources.ListStringResource.GetString(resourceKey);
            KeySelector = keySelector;
            IsReverse = isReverse;
        }

        public string Name { get; }
        public MusicDynamicSortKeySelector KeySelector { get; }
        public bool IsReverse { get; }
    }
}