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

            _musicFileService.FilesAdded += MusicFileService_FilesAdded;
            _musicFileService.FilesRemoved += MusicFileService_FilesRemoved;

            _lyricFileService.FilesAdded += LyricFileService_FilesAdded;
            _lyricFileService.FilesRemoved += LyricFileService_FilesRemoved;
        }

        public async Task<List<LyricIndex>> GetFiles()
        {
            if (_source == null)
                _source = await _helper.ToList();

            return _source;
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

                    table.Update(index);
                    isUpdate = true;
                }
                else
                {
                    _source.Add(index);
                    table.Add(index);
                }
            });

            if (isUpdate)
                FilesUpdated?.Invoke(this, new[] {index});
            else
                FilesAdded?.Invoke(this, new[] {index});
        }
        
        public async Task ScanAsync()
        {
            List<LyricIndex> optionResult = new List<LyricIndex>();

            using (var db = new FilesContext())
            {
                var musicTable = db.MusicFiles;
                var lyricTable = db.LyricFiles;
                var lyricIndexTable = db.LyricIndices;

                /*
                 * 无效项定义：所记录的 音乐/歌词 路径不存在于 音乐/歌词 表中
                 *
                 * 需求：
                 * 1. 找出在 音乐和歌词 表中具有相同名称项，同时还要排除索引表里已有的项
                 * 2. 移除无效项
                 */


                this.LogByObject("移除无效项");
                lyricIndexTable.RemoveRange(lyricIndexTable.Where(li =>
                    musicTable.All(mf => mf.Path != li.MusicPath) || lyricTable.All(lf => lf.Path != li.LyricPath)));

                this.LogByObject("过滤出在 音乐/歌词 表中名称一样的项，并筛选出索引表中没有的项");
                var option = musicTable.Where(m => lyricTable.Any(l => TrimExtensionName(l.FileName) == TrimExtensionName(m.FileName)))
                            // 选取 “lyricIndex” 表里没有的项目
                            .ToList().Where(m => lyricIndexTable.All(i => i.MusicPath != m.Path)).ToList();
                if (option.Any())
                {
                    this.LogByObject("开始建立索引");
                    foreach (var musicFile in option)
                    {
                        LyricFile lyricFile = lyricTable.First(l => TrimExtensionName(l.FileName) == TrimExtensionName(musicFile.FileName));
                        optionResult.Add(new LyricIndex(musicFile.Path, lyricFile.Path));
                    }

                    this.LogByObject("将建立好的索引添加到数据库");
                    await lyricIndexTable.AddRangeAsync(optionResult);
                    await db.SaveChangesAsync();
                }
            }

            if (optionResult.Any())
            {
                this.LogByObject("触发添加事件");
                FilesAdded?.Invoke(this, optionResult);
            }
        }

        private async Task IntelligentAdd<TRelativeTable>(IEnumerable<ILibraryFile> source, Func<FilesContext, DbSet<TRelativeTable>> tableGetter) where TRelativeTable : class, ILibraryFile
        {
            List<ILibraryFile> sourceList = source.ToList();
            List<LyricIndex> optionResult = new List<LyricIndex>();

            using (var db = new FilesContext())
            {
                var relativeTable = tableGetter.Invoke(db);
                var lyricIndexTable = db.LyricIndices;

                var option = relativeTable.Where(l => sourceList.Any(s => TrimExtensionName(s.FileName) == TrimExtensionName(l.FileName))).ToList();

                if (option.Any())
                {
                    foreach (var relativeFile in option)
                    {
                        ILibraryFile sourceFile = sourceList.First(f => TrimExtensionName(f.FileName) == TrimExtensionName(relativeFile.FileName));
                        optionResult.Add(new LyricIndex(sourceFile.Path, relativeFile.Path));
                    }

                    await lyricIndexTable.AddRangeAsync(optionResult);
                    await db.SaveChangesAsync();
                }
            }

            if (optionResult.Any())
                FilesAdded?.Invoke(this, optionResult);
        }

        private async Task IntelligentRemove(IEnumerable<ILibraryFile> source, Func<LyricIndex, string> pathGetter)
        {
            var sourceList = source.ToList();
            List<LyricIndex> optionResult = new List<LyricIndex>();

            await _helper.CustomOption(table =>
            {
                var options = table.Where(i => sourceList.Any(f => f.Path == pathGetter.Invoke(i)));
                if (options.Any())
                {
                    optionResult.AddRange(options);
                    table.RemoveRange(options);
                }
            });

            if (optionResult.Any())
                FilesRemoved?.Invoke(this, optionResult);
        }

        private string TrimExtensionName(string input)
        {
            var blocks = input.Split('.');
            var result = blocks.ToList();
            if (result.Count > 1)
                result.Remove(result.Last());

            return String.Join(".", result).Trim();
        }

        public static async Task<LyricIndexService> GetService()
        {
            if (current == null)
                current = new LyricIndexService(await MusicLibraryService<MusicFile, MusicFileFactory>.GetService(),
                    await MusicLibraryService<LyricFile, LyricFileFactory>.GetService());

            return current;
        }

        private async void MusicFileService_FilesAdded(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("查询与新增的音乐匹配的歌词");
            await IntelligentAdd(e, db => db.LyricFiles);
        }

        private async void MusicFileService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("查询与删除的音乐匹配的索引");
            await IntelligentRemove(e, i => i.MusicPath);
        }

        private async void LyricFileService_FilesAdded(object sender, IEnumerable<LyricFile> e)
        {
            this.LogByObject("查询与新增的歌词匹配的音乐");
            await IntelligentAdd(e, db => db.MusicFiles);
        }

        private async void LyricFileService_FilesRemoved(object sender, IEnumerable<LyricFile> e)
        {
            this.LogByObject("查询与删除的歌词匹配的索引");
            await IntelligentRemove(e, i => i.LyricPath);
        }
    }
}