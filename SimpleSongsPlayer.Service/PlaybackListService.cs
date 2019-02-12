using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Log;

namespace SimpleSongsPlayer.Service
{
    public class PlaybackListService : IDataService<PlaybackItem>
    {
        private static PlaybackListService Current;

        private readonly StorageLibrary _musicLibrary;
        private readonly ContextHelper<FilesContext, PlaybackItem> _helper = new ContextHelper<FilesContext, PlaybackItem>();
        private List<PlaybackItem> _source;

        public PlaybackListService(StorageLibrary musicLibrary)
        {
            _musicLibrary = musicLibrary;
        }

        public event EventHandler<IEnumerable<PlaybackItem>> DataAdded;
        public event EventHandler<IEnumerable<PlaybackItem>> DataRemoved;

        public async Task<List<PlaybackItem>> GetData()
        {
            if (_source is null)
            {
                this.LogByObject("获取数据");
                _source = await _helper.ToList();
            }

            return _source;
        }

        public async Task<List<PlaybackItem>> Add(string path) => await AddRange(new[] {path});

        public async Task Remove(string path) => await RemoveRange(new[] {path});

        public async Task<List<PlaybackItem>> SetUp(IEnumerable<string> paths)
        {
            if (_source is null)
                await GetData();
            
            await RemoveRange(_source.Select(i => i.Path).ToList());
            return await AddRange(paths);
        }

        public async Task<List<PlaybackItem>> AddRange(IEnumerable<string> paths)
        {
            if (_source is null)
                await GetData();

            var result = new List<PlaybackItem>();

            this.LogByObject("正在去重");
            var needAdd = paths.Where(p => _source.All(pb => pb.Path != p)).ToList();
            if (!needAdd.Any())
                return result;

            var libraryPath = _musicLibrary.Folders.Select(d => d.Path).ToList();
            foreach (var item in needAdd)
            {
                if (libraryPath.Any(item.Contains))
                    result.Add(new PlaybackItem(item, FileSourceMembers.MusicLibrary));
                else
                    result.Add(new PlaybackItem(item, FileSourceMembers.Other));
            }

            this.LogByObject("正在应用添加操作");
            _source.AddRange(result);
            await _helper.AddRange(result);
            DataAdded?.Invoke(this, result);
            return result;
        }

        private async Task RemoveRange(IEnumerable<string> paths)
        {
            if (_source is null)
                await GetData();

            this.LogByObject("正在获取有效的路径");
            var needRemovePaths = paths.Where(p => _source.Any(pb => pb.Path == p)).ToList();
            if (!needRemovePaths.Any())
                return;

            this.LogByObject("过滤出需要移除的播放项");
            var needRemove = _source.Where(pb => needRemovePaths.Any(p => p == pb.Path)).ToList();

            this.LogByObject("应用移除操作");
            _source.RemoveAll(needRemove.Contains);
            await _helper.RemoveRange(needRemove);
            DataRemoved?.Invoke(this, needRemove);
        }

        public static async Task<PlaybackListService> GetService()
        {
            if (Current is null)
            {
                typeof(PlaybackListService).LogByType("创建服务");
                Current = new PlaybackListService(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music));
            }

            return Current;
        }
    }
}