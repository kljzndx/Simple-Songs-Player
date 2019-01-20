using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class MusicLibraryFileServiceManager
    {
        private static MusicLibraryFileServiceManager _current;
        private StorageLibrary _musicLibrary;

        private MusicLibraryFileService<MusicFile, MusicFileFactory> _musicFilesService;
        private MusicLibraryFileService<LyricFile, LyricFileFactory> _lyricFilesService;

        private MusicLibraryFileServiceManager(StorageLibrary musicLibrary)
        {
            _musicLibrary = musicLibrary;
        }

        public MusicLibraryFileService<MusicFile, MusicFileFactory> GetMusicFileService()
        {
            if (_musicFilesService is null)
            {
                MusicLibraryFileService<MusicFile, MusicFileFactory>.SetupFileTypeFilter("mp3", "aac", "wav", "flac", "alac", "m4a");
                _musicFilesService =  MusicLibraryFileService<MusicFile, MusicFileFactory>.GetService(_musicLibrary);
            }

            return _musicFilesService;
        }

        public MusicLibraryFileService<LyricFile, LyricFileFactory> GetLyricFileService()
        {
            if (_lyricFilesService is null)
            {
                MusicLibraryFileService<LyricFile, LyricFileFactory>.SetupFileTypeFilter("lrc");
                _lyricFilesService = MusicLibraryFileService<LyricFile, LyricFileFactory>.GetService(_musicLibrary);
            }

            return _lyricFilesService;
        }

        public async Task ScanFiles()
        {
            try
            {
                this.LogByObject("初始化 ChangeTracker");
                _musicLibrary.ChangeTracker.Enable();
                this.LogByObject("获取 ChangeReader");
                var changeReader = _musicLibrary.ChangeTracker.GetChangeReader();
                this.LogByObject("获取更改列表");
                var readBatch = await changeReader.ReadBatchAsync();
                if (readBatch.Any(b => b.ChangeType == StorageLibraryChangeType.ChangeTrackingLost))
                {
                    this.LogByObject("重置 ChangeTracker");
                    _musicLibrary.ChangeTracker.Reset();
                }

                this.LogByObject("开始扫描 ChangeTracker");
                await GetMusicFileService().ScanFiles(readBatch);
                await GetLyricFileService().ScanFiles(readBatch);

                this.LogByObject("向 ChangeTracker 报告已扫描项目");
                await changeReader.AcceptChangesAsync();
            }
            catch (Exception e)
            {
                this.LogByObject(e, "获取/扫描 ChangeTracker 失败");
                //if (e.HResult != -2146958814)
                //    throw;
            }

            this.LogByObject("开始扫描音乐库");
            await GetMusicFileService().ScanFiles();
            await GetLyricFileService().ScanFiles();
        }

        public static async Task<MusicLibraryFileServiceManager> GetManager()
        {
            if (_current is null)
            {
                typeof(MusicLibraryFileServiceManager).LogByType("正在构造服务，正在获取音乐库引用");
                _current = new MusicLibraryFileServiceManager(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music));
            }

            return _current;
        }
    }
}