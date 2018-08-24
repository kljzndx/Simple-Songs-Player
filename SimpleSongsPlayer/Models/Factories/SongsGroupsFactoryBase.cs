using System.Collections.Generic;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public abstract class SongsGroupsFactoryBase
    {
        public abstract List<SongsGroup> ClassifySongsGroups(IEnumerable<Song> allSongs);
    }
}