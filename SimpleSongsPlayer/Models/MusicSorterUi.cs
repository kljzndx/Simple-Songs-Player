using System;
using Windows.ApplicationModel.Resources;
using Windows.Globalization.Collation;

namespace SimpleSongsPlayer.Models
{
    public delegate IComparable MusicDynamicSortKeySelector<T>(T data);

    public class MusicSorterUi<T>
    {
        public MusicSorterUi(string resourceKey, MusicDynamicSortKeySelector<T> keySelector, bool isReverse = false)
        {
            Name = StringResources.SorterMembersStringResource.GetString(resourceKey);
            KeySelector = keySelector;
            IsReverse = isReverse;
        }

        public string Name { get; }
        public MusicDynamicSortKeySelector<T> KeySelector { get; }
        public bool IsReverse { get; }
    }
}