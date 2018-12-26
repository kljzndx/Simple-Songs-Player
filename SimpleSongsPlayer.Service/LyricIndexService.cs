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

        public Task<List<LyricIndex>> GetFiles()
        {
            return _helper.ToList();
        }

        public async Task SetIndex(string musicPath, string lyricPath)
        {
            bool isUpdate = false;
            LyricIndex index = new LyricIndex(musicPath, lyricPath);
            await _helper.CustomOption(table =>
            {
                if (table.Any(i => i.MusicPath == musicPath))
                {
                    isUpdate = true;
                    table.Update(index);
                }
                else
                    table.Add(index);
            });

            if (isUpdate)
                FilesUpdated?.Invoke(this, new[] {index});
            else
                FilesAdded?.Invoke(this, new[] {index});
        }

        public async Task RemoveIndex(string musicPath)
        {
            await _helper.CustomOption(table =>
            {
                var index = table.Find(musicPath);
                if (index != null)
                    table.Remove(index);
            });
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

                // 移除无效的项目
                lyricIndexTable.RemoveRange(lyricIndexTable.Where(li => musicTable.All(mf => li.MusicPath != mf.Path) ||
                                                                        lyricTable.All(lf => li.MusicPath != lf.Path)));

                var option = musicTable.Where(m => lyricTable.Any(l => TrimExtensionName(l.FileName) == TrimExtensionName(m.FileName)))
                            // 选取 “lyricIndex” 表里没有的项目
                            .ToList().Where(o => lyricIndexTable.All(i => i.MusicPath != o.Path));

                foreach (var musicFile in option)
                {
                    LyricFile lyricFile = lyricTable.First(l => TrimExtensionName(l.FileName) == TrimExtensionName(musicFile.FileName));
                    optionResult.Add(new LyricIndex(musicFile.Path, lyricFile.Path));
                }

                await lyricIndexTable.AddRangeAsync(optionResult);
                await db.SaveChangesAsync();
            }

            if (optionResult.Any())
                FilesAdded?.Invoke(this, optionResult);
        }

        private async Task IntelligentlyAdd(IEnumerable<MusicFile> source)
        {
            List<MusicFile> sourceList = source.ToList();
            List<LyricIndex> optionResult = new List<LyricIndex>();

            using (var db = new FilesContext())
            {
                var lyricTable = db.LyricFiles;
                var lyricIndexTable = db.LyricIndices;

                var option = lyricTable.Where(l => sourceList.Any(m => TrimExtensionName(m.FileName) == TrimExtensionName(l.FileName)));
                foreach (var lyricFile in option)
                {
                    MusicFile musicFile = sourceList.First(f => TrimExtensionName(f.FileName) == TrimExtensionName(lyricFile.FileName));
                    optionResult.Add(new LyricIndex(musicFile.Path, lyricFile.Path));
                }

                await lyricIndexTable.AddRangeAsync(optionResult);
                await db.SaveChangesAsync();
            }

            if (optionResult.Any())
            FilesAdded?.Invoke(this, optionResult);
        }

        private async Task IntelligentlyAdd(IEnumerable<LyricFile> source)
        {
            List<LyricFile> sourceList = source.ToList();
            List<LyricIndex> optionResult = new List<LyricIndex>();

            using (var db = new FilesContext())
            {
                var musicTable = db.MusicFiles;
                var lyricIndexTable = db.LyricIndices;

                var option = musicTable.Where(m => sourceList.Any(l => TrimExtensionName(l.FileName) == TrimExtensionName(m.FileName)));
                foreach (var musicFile in option)
                {
                    LyricFile lyricFile = sourceList.First(f => TrimExtensionName(f.FileName) == TrimExtensionName(musicFile.FileName));
                    optionResult.Add(new LyricIndex(musicFile.Path, lyricFile.Path));
                }
                
                await lyricIndexTable.AddRangeAsync(optionResult);
                await db.SaveChangesAsync();
            }

            if (optionResult.Any())
                FilesAdded?.Invoke(this, optionResult);
        }

        private async Task IntelligentlyRemove(IEnumerable<MusicFile> source)
        {
            var sourceList = source.ToList();
            List<LyricIndex> optionResult = new List<LyricIndex>();

            await _helper.CustomOption(table =>
            {
                var options = table.Where(i => sourceList.Any(f => f.Path == i.MusicPath));
                if (options.Any())
                {
                    optionResult.AddRange(options);
                    table.RemoveRange(options);
                }
            });

            if (optionResult.Any())
                FilesRemoved?.Invoke(this, optionResult);
        }

        private async Task IntelligentlyRemove(IEnumerable<LyricFile> source)
        {
            var sourceList = source.ToList();
            List<LyricIndex> optionResult = new List<LyricIndex>();

            await _helper.CustomOption(table =>
            {
                var options = table.Where(i => sourceList.Any(f => f.Path == i.LyricPath));
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
            await IntelligentlyAdd(e);
        }

        private async void MusicFileService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            await IntelligentlyRemove(e);
        }

        private async void LyricFileService_FilesAdded(object sender, IEnumerable<LyricFile> e)
        {
            await IntelligentlyAdd(e);
        }

        private async void LyricFileService_FilesRemoved(object sender, IEnumerable<LyricFile> e)
        {
            await IntelligentlyRemove(e);
        }
    }
}