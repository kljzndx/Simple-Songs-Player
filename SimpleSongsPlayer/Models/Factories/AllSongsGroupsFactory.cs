using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Globalization.Collation;
using Windows.UI.Xaml.Controls;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public  class AllSongsGroupsFactory : SongsGroupsFactoryBase
    {
        private static List<SongsGroup> CreateDefaultGroup(CharacterGroupings cgs)
        {
            var result = cgs.Where(c => !String.IsNullOrWhiteSpace(c.Label))
                .Select(c => c.Label == "..." ? new SongsGroup("?") : new SongsGroup(c.Label)).ToList();

            return result;
        }

        public override List<SongsGroup> ClassifySongsGroups(IEnumerable<Song> allSongs)
        {
            var cgs = new CharacterGroupings();
            var defaultGroup = CreateDefaultGroup(cgs);

            foreach (Song song in allSongs.ToList())
            {
                string label = cgs.Lookup(song.Title);
                int groupId = defaultGroup.FindIndex(c => c.Name.Equals(label, StringComparison.CurrentCulture));
                if (groupId != -1)
                    defaultGroup[groupId].Items.Add(song);
                else
                    defaultGroup.Add(new SongsGroup(label, new List<Song> {song}));
            }

            return defaultGroup;
        }
    }
}