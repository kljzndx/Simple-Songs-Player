using System;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;

namespace SimpleSongsPlayer.ViewModels.Factories
{
    public class MusicGrouperArgs
    {
        public MusicGrouperArgs(IMusicGrouper grouper, IMusicFilter itemFilter)
        {
            Grouper = grouper ?? throw new ArgumentNullException(nameof(grouper));
            ItemFilter = itemFilter ?? throw new ArgumentNullException(nameof(itemFilter));
        }

        public IMusicGrouper Grouper { get; }
        public IMusicFilter ItemFilter { get; }
    }
}