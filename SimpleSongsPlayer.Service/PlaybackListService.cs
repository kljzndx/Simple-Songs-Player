using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Log;

namespace SimpleSongsPlayer.Service
{
    public class PlaybackListService : IDataService<PlaybackItem>
    {
        private static PlaybackListService Current;

        private readonly ContextHelper<FilesContext, PlaybackItem> _helper = new ContextHelper<FilesContext, PlaybackItem>();
        private List<PlaybackItem> _source;

        public PlaybackListService(IDataService<MusicFile> musicDataService)
        {
            musicDataService.DataRemoved += MusicDataService_DataRemoved;
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

        public async Task Add(string path, FileSourceMembers fileSource) => await AddRange(new[] {path}, fileSource);

        public async Task AddRange(IEnumerable<string> paths, FileSourceMembers fileSource)
        {
            if (_source is null)
                await GetData();

            this.LogByObject("正在去重");
            var result = paths.Where(p => _source.All(pb => pb.Path != p)).Select(p => new PlaybackItem(p, fileSource)).ToList();
            if (!result.Any())
                return;

            this.LogByObject("正在应用添加操作");
            _source.AddRange(result);
            await _helper.AddRange(result);
            DataAdded?.Invoke(this, result);
        }

        public async Task Remove(string path) => await RemoveRange(new[] {path});

        public async Task RemoveRange(IEnumerable<string> paths)
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
                Current = new PlaybackListService(await MusicLibraryFileServiceManager.Current.GetMusicFileService());
            }

            return Current;
        }

        private async void MusicDataService_DataRemoved(object sender, IEnumerable<MusicFile> e)
        {
            await RemoveRange(e.Select(f => f.Path));
        }
    }
}