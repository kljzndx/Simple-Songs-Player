using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SimpleSongsPlayer.DataModel;

namespace SimpleSongsPlayer.Models.Factories
{
    public interface ISongGroupsAsyncFactory
    {
        Task<ObservableCollection<SongsGroup>> ClassifySongGroupsAsync(IEnumerable<Song> allSongs);
    }
}