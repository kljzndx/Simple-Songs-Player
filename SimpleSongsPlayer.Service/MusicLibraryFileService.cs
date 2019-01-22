using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Log;

namespace SimpleSongsPlayer.Service
{
    public class MusicLibraryFileService<TFile, TFileFactory> : IFileService<TFile> where TFile : class, ILibraryFile where TFileFactory : ILibraryFileFactory<TFile>, new()
    {
        private const string DBVersion = "V3";

        private static string[] _fileTypeFilter;
        private static MusicLibraryFileService<TFile, TFileFactory> Current;

        private readonly QueryOptions scanOptions;
        private readonly ContextHelper<FilesContext, TFile> helper = new ContextHelper<FilesContext, TFile>();
        private readonly TFileFactory fileFactory = new TFileFactory();
        private readonly StorageLibrary library;
        private List<TFile> musicFiles;

        public event EventHandler<IEnumerable<TFile>> FilesAdded;
        public event EventHandler<IEnumerable<TFile>> FilesRemoved;
        public event EventHandler<IEnumerable<TFile>> FilesUpdated;
        
        private MusicLibraryFileService(StorageLibrary library, string[] fileTypeFilter)
        {
            this.library = library;

            this.LogByObject("正在配置文件筛选器");
            List<string> filters = new List<string>();
            foreach (var filter in fileTypeFilter)
                filters.Add(filter[0] == '.' ? filter : $".{filter}");

            scanOptions = new QueryOptions(CommonFileQuery.OrderByName, filters);
            scanOptions.FolderDepth = FolderDepth.Deep;

            this.LogByObject("构造完成");
        }

        public async Task<List<TFile>> GetFiles()
        {
            if (musicFiles is null)
            {
                this.LogByObject("提取数据库里的数据");
                musicFiles = await helper.ToList();
            }
            
            return musicFiles.ToList();
        }

        internal async Task ScanFiles()
        {
            if (musicFiles is null)
                await GetFiles();
            
            this.LogByObject("移除垃圾数据");
            var folderPaths = library.Folders.Select(d => d.Path).ToList();
            await RemoveRange(musicFiles.Where(f => !folderPaths.Contains(f.LibraryFolder)));

            foreach (var folder in library.Folders)
            {
                this.LogByObject("获取库文件夹里的所有文件索引");
                StorageFileQueryResult query = folder.CreateFileQueryWithOptions(scanOptions);
                uint hasReadCount = 0, fileCount = await query.GetItemCountAsync();
                var allFilePaths = new List<string>();
                
                while (hasReadCount < fileCount)
                {
                    this.LogByObject("获取100个文件");
                    var files = await query.GetFilesAsync(hasReadCount, 100);
                    hasReadCount += (uint) files.Count;
                    allFilePaths.AddRange(files.Select(f => f.Path));

                    {
                        this.LogByObject("筛选出需要添加的文件");
                        List<TFile> addFiles = new List<TFile>();
                        var needAdd = files.Where(f => musicFiles.All(mf => mf.Path != f.Path));

                        this.LogByObject("解析文件");
                        foreach (var file in needAdd)
                            addFiles.Add(await fileFactory.FromStorageFile(folder.Path, file, DBVersion));

                        if (addFiles.Any())
                        {
                            this.LogByObject("应用添加操作");
                            await AddFileRange(addFiles);
                        }
                    }
                    {
                        var needUpdate = new List<TFile>();
                        this.LogByObject("筛选出需要更新数据的文件");
                        foreach (var file in files)
                        {
                            var myFile = musicFiles.FirstOrDefault(f => f.Path == file.Path);
                            if (myFile is null || needUpdate.Any(f => f.Path == file.Path))
                                continue;

                            var prop = await file.GetProperties(async f => await f.GetBasicPropertiesAsync());
                            if (myFile.ChangeDate != prop.DateModified.DateTime)
                                needUpdate.Add(await fileFactory.FromStorageFile(folder.Path, file, DBVersion));
                        }

                        if (needUpdate.Any())
                        {
                            this.LogByObject("应用更新操作");
                            await UpdateFileRange(needUpdate);
                        }
                    }
                }

                this.LogByObject("筛选出要移除的数据项");
                var needRemove = musicFiles.Where(f => f.LibraryFolder == folder.Path)
                    .Where(f => allFilePaths.All(p => p != f.Path)).ToList();

                if (needRemove.Any())
                {
                    this.LogByObject("应用移除操作");
                    await RemoveRange(needRemove);
                }
            }

            var needUpdateOfOldData = new List<TFile>();
            this.LogByObject("扫描陈旧数据");
            foreach (var file in musicFiles.Where(f => f.DBVersion != DBVersion))
                needUpdateOfOldData.Add(await fileFactory.FromFilePath(file.LibraryFolder, file.Path, DBVersion));

            if (needUpdateOfOldData.Any())
            {
                this.LogByObject("更新陈旧数据");
                await UpdateFileRange(needUpdateOfOldData);
            }
        }

        public async Task ScanFiles(IEnumerable<StorageLibraryChange> libraryChanges)
        {
            var changes = libraryChanges.Where(c => c.IsOfType(StorageItemTypes.File) && CheckExtensionName(c.Path));
            var source = new Stack<StorageLibraryChange>(changes);
            var option = new List<StorageLibraryChange>();
            for (int i = 0; i < 100; i++)
                if (source.Any())
                    option.Add(source.Pop());
                else break;

            List<TFile> addFiles = new List<TFile>();
            List<TFile> removeFiles = new List<TFile>();
            List<TFile> updateFiles = new List<TFile>();

            foreach (var libraryChange in option)
            {
                switch (libraryChange.ChangeType)
                {
                    case StorageLibraryChangeType.MovedIntoLibrary:
                    case StorageLibraryChangeType.Created:
                        if (musicFiles.Any(f => f.Path == libraryChange.Path))
                            break;
                        
                        addFiles.Add(await fileFactory.FromFilePath(GrtLibraryFolderPath(libraryChange.Path), libraryChange.Path, DBVersion));
                        break;
                    case StorageLibraryChangeType.MovedOutOfLibrary:
                    case StorageLibraryChangeType.Deleted:
                        if (musicFiles.All(f => f.Path != libraryChange.Path))
                            break;

                        removeFiles.Add(musicFiles.Find(f => f.Path == libraryChange.Path));
                        break;
                    case StorageLibraryChangeType.MovedOrRenamed:
                        if (musicFiles.All(f => f.Path != libraryChange.PreviousPath))
                            break;

                        removeFiles.Add(musicFiles.Find(f => f.Path == libraryChange.PreviousPath));
                        addFiles.Add(await fileFactory.FromFilePath(GrtLibraryFolderPath(libraryChange.Path), libraryChange.Path, DBVersion));
                        break;
                    case StorageLibraryChangeType.ContentsChanged:
                    case StorageLibraryChangeType.ContentsReplaced:
                        if (musicFiles.All(f => f.Path != libraryChange.Path))
                            break;

                        updateFiles.Add(await fileFactory.FromFilePath(GrtLibraryFolderPath(libraryChange.Path), libraryChange.Path, DBVersion));
                        break;
                }
            }

            if (addFiles.Any())
            {
                this.LogByObject("应用添加操作");
                await AddFileRange(addFiles);
            }

            if (removeFiles.Any())
            {
                this.LogByObject("应用移除操作");
                await RemoveRange(removeFiles);
            }

            if (updateFiles.Any())
            {
                this.LogByObject("应用更新操作");
                await UpdateFileRange(updateFiles);
            }

            if (source.Any())
                await ScanFiles(source);
        }

        private bool CheckExtensionName(string filePath)
        {
            var extensionName = filePath.Split('.').Last();
            return scanOptions.FileTypeFilter.Contains($".{extensionName}");
        }

        private string GrtLibraryFolderPath(string filePath)
        {
            var myLibraryFolders = musicFiles.Select(f => f.LibraryFolder);
            var systemLibraryFolders = library.Folders.Select(d => d.Path);
            List<string> allFolders = new List<string>();

            foreach (var folderPath in myLibraryFolders)
                if (allFolders.TrueForAll(d => d != folderPath))
                    allFolders.Add(folderPath.Trim());

            foreach (var folderPath in systemLibraryFolders)
                if (allFolders.TrueForAll(d => d != folderPath))
                    allFolders.Add(folderPath.Trim());

            return allFolders.Where(filePath.Contains).OrderByDescending(d => d.Length).First();
        }

        private async Task AddFileRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);
            
            this.LogByObject("正在添加到数据库");
            await helper.AddRange(filesList);
            this.LogByObject("正在添加到私有列表");
            musicFiles.AddRange(filesList);

            this.LogByObject("触发 ‘添加完成’ 事件");
            FilesAdded?.Invoke(this, filesList);
        }

        private async Task UpdateFileRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);
            
            this.LogByObject("正在对数据库更新");
            await helper.UpdateRange(filesList);
            this.LogByObject("正在对私有列表更新");

            musicFiles.RemoveAll(mf => filesList.Any(f => f.Path == mf.Path));
            musicFiles.AddRange(filesList);

            this.LogByObject("触发 ‘更新完成’ 事件");
            FilesUpdated?.Invoke(this, filesList);
        }

        private async Task RemoveRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);
            
            this.LogByObject("正在从数据库移除");
            await helper.RemoveRange(filesList);
            this.LogByObject("正在从私有列表移除");
            musicFiles.RemoveAll(filesList.Contains);

            this.LogByObject("触发 ‘移除完成’ 事件");
            FilesRemoved?.Invoke(this, filesList);
        }

        internal static void SetupFileTypeFilter(params string[] fileTypeFilter)
        {
            var type = typeof(MusicLibraryFileService<TFile, TFileFactory>);
            if (_fileTypeFilter is null)
            {
                type.LogByType("设置文件筛选器");
                _fileTypeFilter = fileTypeFilter;
            }
        }

        internal static MusicLibraryFileService<TFile, TFileFactory> GetService(StorageLibrary musicLibrary)
        {
            var type = typeof(MusicLibraryFileService<TFile, TFileFactory>);
            if (_fileTypeFilter is null)
            {
                var ex = new Exception("Filter has not set up. Please use 'SetupFileTypeFilter' method to set filter first");
                type.LogByType(ex);
                throw ex;
            }

            if (Current is null)
            {
                type.LogByType("创建服务实例");
                Current = new MusicLibraryFileService<TFile, TFileFactory>(musicLibrary, _fileTypeFilter);
            }

            return Current;
        }
    }
}
