using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;
using NLog;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Service.Models;

namespace SimpleSongsPlayer.Service
{
    public class MusicLibraryService<TFile, TFileFactory> : IFileService<TFile> where TFile : class, ILibraryFile where TFileFactory : ILibraryFileFactory<TFile>, new()
    {
        private static string[] _fileTypeFilter;
        private static MusicLibraryService<TFile, TFileFactory> Current;

        private readonly QueryOptions scanOptions;
        private readonly StorageLibrary musicLibrary;
        private readonly ContextHelper<FilesContext, TFile> helper = new ContextHelper<FilesContext, TFile>();
        private readonly TFileFactory fileFactory = new TFileFactory();
        private List<TFile> musicFiles;

        public event EventHandler<IEnumerable<TFile>> FilesAdded;
        public event EventHandler<IEnumerable<TFile>> FilesRemoved;
        public event EventHandler<IEnumerable<TFile>> FilesUpdated;
        
        private MusicLibraryService(StorageLibrary library, string[] fileTypeFilter)
        {
            this.LogByObject("正在构造服务，正在获取音乐库引用");
            musicLibrary = library;

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

        public async Task ScanFiles()
        {
            if (musicFiles is null)
                await GetFiles();
            
            this.LogByObject("开始扫描");
            List<TFile> addFiles = new List<TFile>();
            List<TFile> removeFiles = new List<TFile>();
            List<TFile> updateFiles = new List<TFile>();
            
            if (!musicFiles.Any())
            {
                foreach (var folder in musicLibrary.Folders)
                {
                    var allFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                    this.LogByObject($"正在添加 {allFiles.Count} 个文件");
                    foreach (var file in allFiles)
                        addFiles.Add(await fileFactory.FromStorageFile(folder.Path, file));
                }

                if (addFiles.Any())
                {
                    this.LogByObject("应用添加操作");
                    await AddFileRange(addFiles);
                }

                return;
            }

            var needRemoveFolders = musicFiles.GroupBy(f => f.LibraryFolder)
                .Where(lg => musicLibrary.Folders.All(d => d.Path != lg.Key)).ToList();

            if (needRemoveFolders.Any())
            {
                this.LogByObject($"正在移除 {needRemoveFolders.Count} 个文件夹");
                foreach (var libraryGroups in needRemoveFolders)
                    await RemoveRange(libraryGroups);
            }
            
            this.LogByObject("将数据库里的数据分类");
            var allGroup = musicFiles.GroupBy(f => f.LibraryFolder).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var folder in musicLibrary.Folders)
            {
                this.LogByObject("从数据库获取对应当前文件夹的数据");
                List<TFile> myFiles;
                bool isGetFiles = allGroup.TryGetValue(folder.Path, out myFiles);
                var myFilePaths = myFiles?.Select(f => f.Path).ToList();

                this.LogByObject("从音乐库获取对应当前文件夹的数据");
                var systemFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                var systemFilePaths = systemFiles.Select(f => f.Path).ToList();
                
                if (isGetFiles)
                {
                    var needRemoveFilePaths = myFilePaths.Where(mf => !systemFilePaths.Contains(mf)).ToList();
                    if (needRemoveFilePaths.Any())
                    {
                        this.LogByObject($"正在移除 {needRemoveFolders.Count} 个文件");
                        removeFiles.AddRange(myFiles.Where(f => needRemoveFilePaths.Contains(f.Path)));
                    }
                }
                
                var needAddFiles = systemFiles.Where(sf => !isGetFiles || !myFilePaths.Contains(sf.Path)).ToList();
                if (needAddFiles.Any())
                {
                    this.LogByObject($"正在添加 {needAddFiles.Count} 个文件");
                    foreach (var filePath in needAddFiles)
                        addFiles.Add(await fileFactory.FromStorageFile(folder.Path, filePath));
                }

                if (!isGetFiles)
                    continue;

                this.LogByObject("获取所有音乐库文件的更改日期");
                Dictionary<StorageFile, DateTimeOffset> allProps = new Dictionary<StorageFile, DateTimeOffset>();
                foreach (var file in systemFiles)
                    allProps.Add(file, (await file.GetBasicPropertiesAsync()).DateModified);

                // 从我的文件里选出所有更新时间里没有的项
                this.LogByObject("获取需要更新的项");
                var needUpdateFiles = allProps.Where(d =>
                    myFiles.All(f => !f.ChangeDate.Equals(d.Value.DateTime)) && myFilePaths.Any(p => p == d.Key.Path)).ToList();

                if (needUpdateFiles.Any())
                {
                    this.LogByObject($"正在更新 {needUpdateFiles.Count} 个项的信息");
                    foreach (var item in needUpdateFiles)
                        updateFiles.Add(await fileFactory.FromStorageFile(folder.Path, item.Key));
                }
            }

            if (updateFiles.Any())
            {
                this.LogByObject("应用更新操作");
                await UpdateFileRange(updateFiles);
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
            
            this.LogByObject("完成文件扫描");
        }

        private async Task AddFileRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            Stack<TFile> filesStack = new Stack<TFile>(files);
            List<TFile> filesList = new List<TFile>();

            for (int i = 0; i < 200; i++)
                if (filesStack.Count > 0)
                    filesList.Add(filesStack.Pop());
                else break;

            this.LogByObject("正在添加到数据库");
            await helper.AddRange(filesList);
            this.LogByObject("正在添加到私有列表");
            musicFiles.AddRange(filesList);
            this.LogByObject("触发 ‘添加完成’ 事件");
            FilesAdded?.Invoke(this, filesList);

            if (filesStack.Count > 0)
                await AddFileRange(filesStack);
        }

        private async Task UpdateFileRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            Stack<TFile> filesStack = new Stack<TFile>(files);
            List<TFile> filesList = new List<TFile>();

            for (int i = 0; i < 200; i++)
                if (filesStack.Count > 0)
                    filesList.Add(filesStack.Pop());
                else break;

            this.LogByObject("正在对数据库更新");
            await helper.UpdateRange(filesList);
            this.LogByObject("正在对私有列表更新");

            var ids = filesList.Select(nf => musicFiles.IndexOf(musicFiles.Find(of => of.Path == nf.Path))).ToList();
            musicFiles.RemoveAll(filesList.Contains);

            for (int i = 0; i < filesList.Count; i++)
                musicFiles.Insert(ids[i], filesList[i]);

            this.LogByObject("触发 ‘更新完成’ 事件");
            FilesUpdated?.Invoke(this, filesList);

            if (filesStack.Count > 0)
                await UpdateFileRange(filesStack);
        }

        private async Task RemoveRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            Stack<TFile> filesStack = new Stack<TFile>(files);
            List<TFile> filesList = new List<TFile>();

            for (int i = 0; i < 200; i++)
                if (filesStack.Count > 0)
                    filesList.Add(filesStack.Pop());
                else break;

            this.LogByObject("正在从数据库移除");
            await helper.RemoveRange(filesList);
            this.LogByObject("正在从私有列表移除");
            musicFiles.RemoveAll(filesList.Contains);
            this.LogByObject("触发 ‘移除完成’ 事件");
            FilesRemoved?.Invoke(this, filesList);

            if (filesStack.Count > 0)
                await RemoveRange(filesStack);
        }

        public static void SetupFileTypeFilter(params string[] fileTypeFilter)
        {
            var type = typeof(MusicLibraryService<TFile, TFileFactory>);
            if (_fileTypeFilter is null)
            {
                type.LogByType("设置文件筛选器");
                _fileTypeFilter = fileTypeFilter;
            }
        }

        public static async Task<MusicLibraryService<TFile, TFileFactory>> GetService()
        {
            var type = typeof(MusicLibraryService<TFile, TFileFactory>);
            if (_fileTypeFilter is null)
            {
                var ex = new Exception("Filter has not set up. Please use 'SetupFileTypeFilter' method to set filter first");
                type.LogByType(ex);
                throw ex;
            }

            if (Current is null)
            {
                type.LogByType("创建服务实例");
                Current = new MusicLibraryService<TFile, TFileFactory>(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music), _fileTypeFilter);
            }
            return Current;
        }
    }
}
