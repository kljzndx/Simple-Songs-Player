using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class LyricIndexService : IFileService<LyricIndex>
    {
        private static LyricIndexService current;

        private readonly ContextHelper<FilesContext, LyricIndex> _helper = new ContextHelper<FilesContext, LyricIndex>();
        private readonly MusicLibraryService<MusicFile, MusicFileFactory> _musicFileService;
        private readonly MusicLibraryService<LyricFile, LyricFileFactory> _lyricFileService;

        private List<LyricIndex> _source;
        
        public event EventHandler<IEnumerable<LyricIndex>> FilesAdded;
        public event EventHandler<IEnumerable<LyricIndex>> FilesRemoved;
        public event EventHandler<IEnumerable<LyricIndex>> FilesUpdated;

        private LyricIndexService(MusicLibraryService<MusicFile, MusicFileFactory> musicFileService, MusicLibraryService<LyricFile, LyricFileFactory> lyricFileService)
        {
            _musicFileService = musicFileService;
            _lyricFileService = lyricFileService;

            _musicFileService.FilesRemoved += MusicFileService_FilesRemoved;
            
            _lyricFileService.FilesRemoved += LyricFileService_FilesRemoved;
        }

        public async Task<List<LyricIndex>> GetFiles()
        {
            if (_source == null)
            {
                this.LogByObject("提取数据库中的数据");
                _source = await _helper.ToList();
            }

            return _source.ToList();
        }

        public async Task SetIndex(string musicPath, string lyricPath)
        {
            bool isUpdate = false;
            LyricIndex index = new LyricIndex(musicPath, lyricPath);
            await _helper.CustomOption(table =>
            {
                if (table.Any(i => i.MusicPath == musicPath))
                {
                    this.LogByObject("更新歌词路径");
                    var item = _source.Find(i => i.MusicPath == musicPath);
                    item.LyricPath = lyricPath;
                    index = item;

                    this.LogByObject("应用更新操作");
                    table.Update(index);
                    isUpdate = true;
                }
            });

            if (isUpdate)
            {
                this.LogByObject("触发更新事件");
                FilesUpdated?.Invoke(this, new[] {index});
            }
            else
            {
                this.LogByObject("开始添加索引");
                await AddRange(new[] {index});
            }
        }
        
        public async Task ScanAsync()
        {
            if (_source == null)
                await GetFiles();

            this.LogByObject("开始扫描");

            List<MusicFile> musicFiles = await _musicFileService.GetFiles();
            List<LyricFile> lyricFiles = await _lyricFileService.GetFiles();

            List<LyricIndex> addOption = new List<LyricIndex>();
            List<LyricIndex> removeOption = new List<LyricIndex>();

            this.LogByObject("清理无效项");
            foreach (var lyricIndex in _source)
                if (musicFiles.All(f => f.Path != lyricIndex.MusicPath) || lyricFiles.All(f => f.Path != lyricIndex.LyricPath))
                    removeOption.Add(lyricIndex);

            this.LogByObject("提取出所有的文件名");
            var musicFileNames = musicFiles.ToDictionary(f => TrimExtensionName(f.FileName));
            var lyricFileNames = lyricFiles.ToDictionary(f => TrimExtensionName(f.FileName));

            this.LogByObject("查询文件名一样的音乐和歌词项");
            var needMakeIndexes = musicFileNames.Where(md => lyricFileNames.ContainsKey(md.Key) && _source.All(i => i.MusicPath != md.Value.Path)).ToList();
            if (needMakeIndexes.Any())
            {
                this.LogByObject("开始建立索引");
                foreach (var musicNamePair in needMakeIndexes)
                    addOption.Add(new LyricIndex(musicNamePair.Value.Path, lyricFileNames[musicNamePair.Key].Path));
            }


            if (addOption.Any())
            {
                this.LogByObject("应用添加操作");
                await AddRange(addOption);
            }

            if (removeOption.Any())
            {
                this.LogByObject("应用移除操作");
                await RemoveRange(removeOption);
            }
        }

        private string TrimExtensionName(string input)
        {
            this.LogByObject("用 '.' 分割文件名");
            var blocks = input.Split('.').ToList();
            if (blocks.Count > 1)
            {
                this.LogByObject("移除扩展名");
                blocks.Remove(blocks.Last());
            }

            this.LogByObject("重组字符串");
            return String.Join(".", blocks).Trim();
        }

        private async Task AddRange(IEnumerable<LyricIndex> source)
        {
            this.LogByObject("接收数据");
            Stack<LyricIndex> sourceStack = new Stack<LyricIndex>(source);
            List<LyricIndex> optionList = new List<LyricIndex>();
            for (int i = 0; i < 200; i++)
                if (sourceStack.Any())
                    optionList.Add(sourceStack.Pop());
                else break;

            this.LogByObject("添加到数据库");
            await _helper.AddRange(optionList);
            this.LogByObject("添加到私有列表");
            _source.AddRange(optionList);

            this.LogByObject("触发添加事件");
            FilesAdded?.Invoke(this, optionList);

            if (sourceStack.Any())
                await AddRange(sourceStack);
        }

        private async Task RemoveRange(IEnumerable<LyricIndex> source)
        {
            this.LogByObject("接收数据");
            Stack<LyricIndex> sourceStack = new Stack<LyricIndex>(source);
            List<LyricIndex> optionList = new List<LyricIndex>();
            for (int i = 0; i < 200; i++)
                if (sourceStack.Any())
                    optionList.Add(sourceStack.Pop());
                else break;

            this.LogByObject("从数据库移除数据");
            await _helper.RemoveRange(optionList);
            this.LogByObject("从私有列表移除数据");
            _source.RemoveAll(optionList.Contains);

            this.LogByObject("触发移除事件");
            FilesRemoved?.Invoke(this, optionList);

            if (sourceStack.Any())
                await RemoveRange(sourceStack);
        }

        public static async Task<LyricIndexService> GetService()
        {
            if (current == null)
            {
                typeof(LyricIndexService).LogByType("创建服务");
                current = new LyricIndexService(await MusicLibraryService<MusicFile, MusicFileFactory>.GetService(),
                    await MusicLibraryService<LyricFile, LyricFileFactory>.GetService());
            }

            return current;
        }

        private async void MusicFileService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("查询与删除的音乐匹配的索引");
            var indexes = _source.Where(i => e.Any(f => f.Path == i.MusicPath)).ToList();
            if (indexes.Any())
                await RemoveRange(indexes);
        }

        private async void LyricFileService_FilesRemoved(object sender, IEnumerable<LyricFile> e)
        {
            this.LogByObject("查询与删除的歌词匹配的索引");
            var indexes = _source.Where(i => e.Any(f => f.Path == i.LyricPath)).ToList();
            if (indexes.Any())
                await RemoveRange(indexes);
        }
    }
}