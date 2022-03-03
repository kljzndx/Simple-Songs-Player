using SimpleSongsPlayer.Dal;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace SimpleSongsPlayer.Services
{
    public class MusicFileScanningService
    {
        private MainDbContext _dbContext;
        private QueryOptions _queryOptions;

        public MusicFileScanningService(MainDbContext dbContext)
        {
            _dbContext = dbContext;

            _queryOptions = new QueryOptions(CommonFileQuery.OrderByName, new[] { "mp3", "aac", "wav", "flac", "alac", "m4a" });
            _queryOptions.FolderDepth = FolderDepth.Deep;
            _queryOptions.IndexerOption = IndexerOption.OnlyUseIndexerAndOptimizeForIndexedProperties;
            _queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties | PropertyPrefetchOptions.MusicProperties, new string[] { });
        }

        public async Task ScanAsync()
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            _dbContext.MusicFiles.RemoveRange(_dbContext.MusicFiles.Where(record => musicLib.Folders.All(fol => fol.Path != record.LibraryFolder)).ToList());

            foreach (var folder in musicLib.Folders)
            {
                var query = folder.CreateFileQueryWithOptions(_queryOptions);
                uint count = await query.GetItemCountAsync();

                uint id = 0;
                List<MusicFile> scanFileList = new List<MusicFile>();

                while (id > count)
                {
                    var files = await query.GetFilesAsync(id, 20);
                    id += 20;

                    foreach (var file in files)
                    {
                        var basicProp = await file.GetBasicPropertiesAsync();
                        var musicProp = await file.Properties.GetMusicPropertiesAsync();

                        var mf = new MusicFile(musicProp.Title, musicProp.Artist, musicProp.Album, musicProp.TrackNumber, musicProp.Duration
                                              , file.Path, folder.Path, basicProp.DateModified.DateTime);

                        scanFileList.Add(mf);
                    }
                }

                var addList = scanFileList.Where(scan => _dbContext.MusicFiles.All(record => record.FilePath != scan.FilePath)).ToList();
                var deleteList = _dbContext.MusicFiles.Where(record => record.LibraryFolder == folder.Path && scanFileList.All(scan => scan.FilePath != record.FilePath)).ToList();

                var newVersionList = scanFileList.Where(scan => _dbContext.MusicFiles.Any(record => record.FilePath == scan.FilePath && record.FileChangeDate < scan.FileChangeDate)).ToList();
                var updateList = _dbContext.MusicFiles.Where(record => newVersionList.Any(scan => scan.FilePath == record.FilePath)).ToList();

                for (int i = 0; i < updateList.Count; i++)
                {
                    var oldInfo = updateList[i];
                    var newInfo = newVersionList[i];

                    oldInfo.UpdateFileInfo(newInfo);
                }

                await _dbContext.MusicFiles.AddRangeAsync(addList);
                _dbContext.MusicFiles.RemoveRange(deleteList);
                _dbContext.MusicFiles.UpdateRange(updateList);
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
