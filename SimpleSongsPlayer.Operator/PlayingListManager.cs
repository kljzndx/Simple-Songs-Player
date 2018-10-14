using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using SimpleSongsPlayer.DataModel;

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
        }
        
        public ReadOnlyCollection<PlayingListBlock> Blocks => _blocks.AsReadOnly();

		public PlayingListBlock GetBlock(string name) => _blocks.Find(b => b.Name.Equals(name));

		public async Task<PlayingListBlock> CreateBlockAsync(string name)
        {
            var file = await plbsFolder.CreateFileAsync(name, CreationCollisionOption.GenerateUniqueName);
            var block = await PlayingListBlock.CreateFromFileAsync(file);
            _blocks.Add(block);
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

                var options = new QueryOptions(CommonFileQuery.OrderByName, new[] {".plb"})
                {
                    IndexerOption = IndexerOption.OnlyUseIndexerAndOptimizeForIndexedProperties
                };
                options.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties, new[] {SystemProperties.Title, SystemProperties.ItemNameDisplay});

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