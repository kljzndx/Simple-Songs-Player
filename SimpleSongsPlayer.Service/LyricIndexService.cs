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
                _source = await _helper.ToList();

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
                    var item = _source.Find(i => i.MusicPath == musicPath);
                    item.LyricPath = lyricPath;
                    index = item;
                    table.Update(index);
                    isUpdate = true;
                }
            });

            if (isUpdate)
                FilesUpdated?.Invoke(this, new[] {index});
            else
                await AddRange(new[] {index});
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

            foreach (var lyricIndex in _source)
                if (musicFiles.All(f => f.Path != lyricIndex.MusicPath) || lyricFiles.All(f => f.Path != lyricIndex.LyricPath))
                    removeOption.Add(lyricIndex);

            var musicFileNames = musicFiles.ToDictionary(f => TrimExtensionName(f.FileName));
            var lyricFileNames = lyricFiles.ToDictionary(f => TrimExtensionName(f.FileName));

            var needMakeIndexes = musicFileNames.Where(md => lyricFileNames.ContainsKey(md.Key) && _source.All(i => i.MusicPath != md.Value.Path)).ToList();

            foreach (var musicNamePair in needMakeIndexes)
                addOption.Add(new LyricIndex(musicNamePair.Value.Path, lyricFileNames[musicNamePair.Key].Path));

            if (addOption.Any())
                await AddRange(addOption);

            if (removeOption.Any())
                await RemoveRange(removeOption);

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
            Stack<LyricIndex> sourceStack = new Stack<LyricIndex>(source);
            List<LyricIndex> optionList = new List<LyricIndex>();
            for (int i = 0; i < 200; i++)
                if (sourceStack.Any())
                    optionList.Add(sourceStack.Pop());
                else break;

            await _helper.AddRange(optionList);
            _source.AddRange(optionList);

            FilesAdded?.Invoke(this, optionList);

            if (sourceStack.Any())
                await AddRange(sourceStack);
        }

        private async Task RemoveRange(IEnumerable<LyricIndex> source)
        {
            Stack<LyricIndex> sourceStack = new Stack<LyricIndex>(source);
            List<LyricIndex> optionList = new List<LyricIndex>();
            for (int i = 0; i < 200; i++)
                if (sourceStack.Any())
                    optionList.Add(sourceStack.Pop());
                else break;

            await _helper.RemoveRange(optionList);
            _source.RemoveAll(optionList.Contains);

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