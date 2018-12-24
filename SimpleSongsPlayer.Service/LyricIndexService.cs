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
            LyricIndex index = new LyricIndex(musicPath, lyricPath);
            await _helper.CustomOption(table =>
            {
                if (table.Any(i => i.MusicPath == musicPath))
                    table.Update(index);
                else
                    table.Add(index);
            });

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

                lyricIndexTable.RemoveRange(lyricIndexTable.Where(li => musicTable.All(mf => li.MusicPath != mf.Path)));
                lyricIndexTable.RemoveRange(lyricIndexTable.Where(li => lyricTable.All(lf => li.MusicPath != lf.Path)));

                var option = musicTable.Where(m => lyricTable.Any(l => TrimExtensionName(l.FileName) == TrimExtensionName(m.FileName))).ToList();
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