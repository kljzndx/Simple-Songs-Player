﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

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

        public async Task<List<string>> GetPaths()
        {
            if (_paths is null)
                _paths = (await FileIO.ReadLinesAsync(_file)).ToList();
            return _paths;
        }

        public async Task AddPath(string path)
        {
            await AddPaths(new List<string> {path});
        }

        public async Task AddPaths(IEnumerable<string> paths)
        {
            List<string> sourcePaths = await GetPaths();
            List<string> addPaths = paths.Where(s => !sourcePaths.Contains(s)).ToList();
            if (!addPaths.Any())
                return;

            sourcePaths.AddRange(addPaths);
            await FileIO.AppendLinesAsync(_file, addPaths);
            ChangeDate = DateTime.Now;
        }

        public async Task RemovePath(string path)
        {
            await RemovePaths(new List<string> {path});
        }

        public async Task RemovePaths(IEnumerable<string> paths)
        {
            List<string> sourcePaths = await GetPaths();
            List<string> removePaths = paths.Where(s => sourcePaths.Contains(s)).ToList();
            if (!removePaths.Any())
                return;

            foreach (var path in removePaths)
                sourcePaths.Remove(path);

            await FileIO.WriteLinesAsync(_file, sourcePaths);
            ChangeDate = DateTime.Now;
        }

        public static async Task<PlayingListBlock> CreateFromFileAsync(StorageFile file)
        {
            return new PlayingListBlock(file, await file.GetBasicPropertiesAsync());
        }
    }
}