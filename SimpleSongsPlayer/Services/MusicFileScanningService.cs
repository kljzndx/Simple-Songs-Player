using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

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
        private MainDbContext DbContext => Ioc.Default.GetRequiredService<MainDbContext>();
        private QueryOptions _queryOptions;

        public MusicFileScanningService()
        {
            _queryOptions = new QueryOptions(CommonFileQuery.OrderByName, new[] { ".mp3", ".aac", ".wav", ".flac", ".alac", ".m4a" });
            _queryOptions.FolderDepth = FolderDepth.Deep;
            _queryOptions.IndexerOption = IndexerOption.OnlyUseIndexerAndOptimizeForIndexedProperties;
            _queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties | PropertyPrefetchOptions.MusicProperties, new string[] { });
        }

        public async Task ScanAsync()
        {
            WeakReferenceMessenger.Default.Send("Started", nameof(MusicFileScanningService));
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);

            var folderPathList = musicLib.Folders.Select(f => f.Path).ToList();
            var trashList = DbContext.MusicFiles.Where(record => folderPathList.All(path => path != record.LibraryFolder)).ToList();
            DbContext.MusicFiles.RemoveRange(trashList);

            foreach (var folder in musicLib.Folders)
            {
                var query = folder.CreateFileQueryWithOptions(_queryOptions);
                uint count = await query.GetItemCountAsync();

                List<MusicFile> scanFileList = new List<MusicFile>();

                for (uint id = 0; id < count; id += 20)
                {
                    var files = await query.GetFilesAsync(id, 20);

                    foreach (var file in files)
                    {
                        var basicProp = await file.GetBasicPropertiesAsync();
                        var musicProp = await file.Properties.GetMusicPropertiesAsync();

                        var mf = new MusicFile(musicProp.Title, musicProp.Artist, musicProp.Album, musicProp.TrackNumber, musicProp.Duration
                                              , file.Path, folder.Path, basicProp.DateModified.DateTime);

                        scanFileList.Add(mf);
                    }
                }

                var dbData = DbContext.MusicFiles.Where(record => record.LibraryFolder == folder.Path).ToList();

                var addList = scanFileList.Where(scan => dbData.All(record => record.FilePath != scan.FilePath)).ToList();
                var deleteList = dbData.Where(record => scanFileList.All(scan => scan.FilePath != record.FilePath)).ToList();

                var newVersionList = scanFileList.Where(scan => dbData.Any(record => record.FilePath == scan.FilePath && record.FileChangeDate < scan.FileChangeDate)).ToList();
                var oldVersionList = dbData.Where(record => newVersionList.Any(scan => scan.FilePath == record.FilePath)).ToList();
                var updateList = newVersionList.Select(scan => oldVersionList.First(record => record.FilePath == scan.FilePath).UpdateFileInfo(scan)).ToList();

                DbContext.MusicFiles.AddRange(addList);
                DbContext.MusicFiles.RemoveRange(deleteList);
                DbContext.MusicFiles.UpdateRange(updateList);
            }

            await DbContext.SaveChangesAsync();
            WeakReferenceMessenger.Default.Send("Finished", nameof(MusicFileScanningService));
        }
    }
}
