using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.FileProperties;
using SimpleSongsPlayer.DataModel.Events;

namespace SimpleSongsPlayer.DataModel
{
    public class PlayingListBlock : ObservableObject
    {
        private readonly StorageFile _file;
        private List<string> _paths;

        private string name;
        private DateTime changeDate;

        private PlayingListBlock(StorageFile file, BasicProperties bp)
        {
            _file = file;
            name = file.DisplayName;
            changeDate = bp.DateModified.DateTime;
        }

        public string Name
        {
            get => name;
            private set => Set(ref name, value);
        }

        public DateTime ChangeDate
        {
            get => changeDate;
            set => Set(ref changeDate, value);
        }

        public int Count => _paths?.Count ?? 0;

        public event TypedEventHandler<PlayingListBlock, PlayingListBlockRenamedEventArgs> Renamed;

        public async Task<ReadOnlyCollection<string>> GetPaths()
        {
            if (_paths is null)
                _paths = (await FileIO.ReadLinesAsync(_file)).ToList();

            return _paths.AsReadOnly();
        }

        public async Task AddPath(string path)
        {
            await AddPaths(new List<string> {path});
        }

        public async Task AddPaths(IEnumerable<string> paths)
        {
            var sourcePaths = await GetPaths();
            List<string> addPaths = paths.Where(s => !sourcePaths.Contains(s)).ToList();
            if (!addPaths.Any())
                return;

            foreach (var addPath in addPaths)
                _paths.Add(addPath);
            await FileIO.AppendLinesAsync(_file, addPaths);
            ChangeDate = DateTime.Now;
        }

        public async Task RemovePath(string path)
        {
            await RemovePaths(new List<string> {path});
        }

        public async Task RemovePaths(IEnumerable<string> paths)
        {
            var sourcePaths = await GetPaths();
            List<string> removePaths = paths.Where(s => sourcePaths.Contains(s)).ToList();
            if (!removePaths.Any())
                return;

            foreach (var path in removePaths)
                _paths.Remove(path);

            await FileIO.WriteLinesAsync(_file, sourcePaths);
            ChangeDate = DateTime.Now;
        }

        public async Task RenameAsync(string newName)
        {
            string oldName = Name;
            await _file.RenameAsync(newName + ".plb");
            Name = newName;
            ChangeDate = DateTime.Now;

            Renamed?.Invoke(this, new PlayingListBlockRenamedEventArgs(oldName, newName));
        }

        public async Task DeleteFileAsync() => await _file.DeleteAsync();

        public static async Task<PlayingListBlock> CreateFromFileAsync(StorageFile file)
        {
            return new PlayingListBlock(file, await file.GetBasicPropertiesAsync());
        }
    }
}