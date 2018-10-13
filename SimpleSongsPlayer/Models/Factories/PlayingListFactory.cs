using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Operator;

namespace SimpleSongsPlayer.Models.Factories
{
    public class PlayingListFactory : ISongGroupsAsyncFactory
    {
        private PlayingListManager listManager;

        public async Task<ObservableCollection<SongsGroup>> ClassifySongGroupsAsync(IEnumerable<Song> allSongs)
        {
            if (listManager is null)
                listManager = await PlayingListManager.GetManager();

            List<Song> allSongsList = allSongs.ToList();

            ObservableCollection<SongsGroup> result = new ObservableCollection<SongsGroup>();
            ReadOnlyCollection<PlayingListBlock> playingListBlocks = listManager.Blocks;

            foreach (var block in playingListBlocks)
            {
                ReadOnlyCollection<string> paths = await block.GetPaths();
                SongsGroup group = new SongsGroup(block.Name);

                foreach (var path in paths)
                    if (allSongsList.Find(s => s.Path.Equals(path)) is Song song)
                        group.Items.Add(song);
                    else
                        await block.RemovePath(path);

                result.Add(group);
            }

            return result;
        }
    }
}