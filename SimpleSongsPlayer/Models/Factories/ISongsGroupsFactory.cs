using System.Collections.Generic;
using System.Collections.ObjectModel;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public interface ISongsGroupsFactory
    {
        ObservableCollection<SongsGroup> ClassifySongGroups(IEnumerable<Song> allSongs);
    }
}