using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.DataModel.Events;

namespace SimpleSongsPlayer.Operator
{
    public class PlayingListManager
    {
        private static PlayingListManager manager;
        private static StorageFolder plbsFolder;

        private List<PlayingListBlock> _blocks;

        public PlayingListManager()
        {
            _blocks = new List<PlayingListBlock>();
        }

        private PlayingListManager(IEnumerable<PlayingListBlock> blocks)
        {
            _blocks = blocks.ToList();

            foreach (var block in _blocks)
                block.Renamed += (s, e) => BlockRenamed?.Invoke(this, e);
        }

        public ReadOnlyCollection<PlayingListBlock> Blocks => _blocks.AsReadOnly();

        public event TypedEventHandler<PlayingListManager, PlayingListBlock> BlockCreated;
        public event TypedEventHandler<PlayingListManager, PlayingListBlock> BlockDeleted;
        public event TypedEventHandler<PlayingListManager, PlayingListBlockRenamedEventArgs> BlockRenamed;

        public PlayingListBlock GetBlock(string name) => _blocks.Find(b => b.Name.Equals(name));

        public async Task<PlayingListBlock> CreateBlockAsync(string name)
        {
            var file = await plbsFolder.CreateFileAsync(name + ".plb", CreationCollisionOption.GenerateUniqueName);
            var block = await PlayingListBlock.CreateFromFileAsync(file);
            _blocks.Add(block);
            BlockCreated?.Invoke(this, block);
            return block;
        }

        public async Task<PlayingListBlock> CreateBlockAsync(string name, IEnumerable<string> paths)
        {
            var block = await CreateBlockAsync(name);
            await block.AddPaths(paths);
            return block;
        }

        public async Task DeleteBlockAsync(PlayingListBlock block)
        {
            if (!_blocks.Contains(block))
                return;

            await block.DeleteFileAsync();
            _blocks.Remove(block);
            BlockDeleted?.Invoke(this, block);
        }

        public static async Task<PlayingListManager> GetManager()
        {
            if (manager is null)
            {
                var localFolder = ApplicationData.Current.LocalFolder;

                try
                {
                    plbsFolder = await localFolder.GetFolderAsync("plbs");
                }
                catch (Exception)
                {
                    plbsFolder = await localFolder.CreateFolderAsync("plbs");
                    manager = new PlayingListManager();
                    return manager;
                }

                var options = new QueryOptions(CommonFileQuery.OrderByName, new[] {".plb"});

                var query = plbsFolder.CreateFileQueryWithOptions(options);
                var files = await query.GetFilesAsync();

                List<PlayingListBlock> blocksList = new List<PlayingListBlock>();

                foreach (var file in files)
                    blocksList.Add(await PlayingListBlock.CreateFromFileAsync(file));

                manager = new PlayingListManager(blocksList);
            }

            return manager;
        }
    }
}