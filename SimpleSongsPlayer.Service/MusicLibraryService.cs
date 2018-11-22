using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
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

        private readonly QueryOptions scanOptions = new QueryOptions();
        private readonly StorageLibrary musicLibrary;
        private readonly ContextHelper<FilesContext, TFile> helper = new ContextHelper<FilesContext, TFile>();
        private readonly TFileFactory fileFactory = new TFileFactory();
        private readonly List<TFile> musicFiles;

        public event EventHandler<IEnumerable<TFile>> FilesAdded;
        public event EventHandler<IEnumerable<TFile>> FilesRemoved;
        
        private MusicLibraryService(StorageLibrary library, string[] fileTypeFilter)
        {
            this.LogByObject("正在构造服务，正在获取音乐库引用");
            musicLibrary = library;
            this.LogByObject("正在获取文件");
            musicFiles = helper.ToList();

            this.LogByObject("正在配置文件筛选器");
            foreach (var filter in fileTypeFilter)
                scanOptions.FileTypeFilter.Add(filter[0] == '.' ? filter : $".{filter}");

            this.LogByObject("构造完成");
        }

        public List<TFile> GetFiles()
        {
            this.LogByObject("正在输出文件列表");
            return musicFiles.ToList();
        }

        public async Task ScanFiles()
        {
            this.LogByObject("准备 ‘操作集合’");
            List<TFile> addFiles = new List<TFile>();
            List<TFile> removeFiles = new List<TFile>();

            this.LogByObject("检测数据库是否有数据");
            if (!musicFiles.Any())
            {
                this.LogByObject("检测到数据库里没数据，进入收集模式，开始扫描音乐库");
                foreach (var folder in musicLibrary.Folders)
                {
                    var allFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                    foreach (var file in allFiles)
                        addFiles.Add(await fileFactory.FromStorageFile(folder.Name, file));
                }

                this.LogByObject("应用 ‘添加操作集合’");
                if (addFiles.Any())
                    AddFileRange(addFiles);

                this.LogByObject("完成 ‘添加操作集合’ 应用");
                return;
            }

            this.LogByObject("检测到数据库里有数据，进入 ‘对比’ 模式，开始检测音乐库是否有文件夹被移除");
            foreach (var libraryGroups in musicFiles.GroupBy(f => f.LibraryFolder))
                if (musicLibrary.Folders.All(d => d.Name != libraryGroups.Key))
                {
                    this.LogByObject($"已检测到有文件夹已被移除，正在同步至数据库");
                    RemoveRange(libraryGroups);
                    this.LogByObject("完成同步");
                }

            this.LogByObject("将数据库里的数据分类");
            var allGroup = musicFiles.GroupBy(f => f.LibraryFolder).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var folder in musicLibrary.Folders)
            {
                this.LogByObject("从数据库获取对应当前文件夹的数据");
                List<TFile> myFiles;
                bool isGetFiles = allGroup.TryGetValue(folder.Name, out myFiles);
                var myFilePaths = myFiles?.Select(f => f.Path).ToList();

                this.LogByObject("从音乐库获取对应当前文件夹的数据");
                var systemFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                var systemFilePaths = systemFiles.Select(f => f.Path).ToList();

                this.LogByObject("进入对比流程");
                if (isGetFiles)
                {
                    this.LogByObject("获取需要移除的文件");
                    var needRemoveFilePaths = myFilePaths.Where(mf => !systemFilePaths.Contains(mf)).ToList();
                    this.LogByObject("将结果添加至 ‘移除操作’ 集合");
                    removeFiles.AddRange(myFiles.Where(f => needRemoveFilePaths.Contains(f.Path)));
                }

                this.LogByObject("获取需要添加的文件");
                var needAddFiles = systemFiles.Where(sf => !isGetFiles || !myFilePaths.Contains(sf.Path));
                this.LogByObject("将结果添加至 ‘添加操作’ 集合");
                foreach (var filePath in needAddFiles)
                    addFiles.Add(await fileFactory.FromStorageFile(folder.Name, filePath));
            }

            this.LogByObject("应用 ‘操作集合’");
            if (addFiles.Any())
                AddFileRange(addFiles);

            if (removeFiles.Any())
                RemoveRange(removeFiles);

            this.LogByObject("完成文件扫描");
        }

        private void AddFileRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);

            this.LogByObject("正在添加到数据库");
            helper.AddRange(filesList);
            this.LogByObject("正在添加到列表");
            musicFiles.AddRange(filesList);
            this.LogByObject("触发 ‘添加完成’ 事件");
            FilesAdded?.Invoke(this, filesList);
        }

        private void RemoveRange(IEnumerable<TFile> files)
        {
            this.LogByObject("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);

            this.LogByObject("正在从数据库移除");
            helper.RemoveRange(filesList);
            this.LogByObject("正在从列表移除");
            musicFiles.RemoveAll(filesList.Contains);
            this.LogByObject("触发 ‘移除完成’ 事件");
            FilesRemoved?.Invoke(this, filesList);
        }

        public static void SetupFileTypeFilter(params string[] fileTypeFilter)
        {
            var type = typeof(MusicLibraryService<TFile, TFileFactory>);
            type.LogByType("设置文件筛选器");
            if (_fileTypeFilter is null)
                _fileTypeFilter = fileTypeFilter;
            else
                type.LogByType("已设置筛选器");
        }

        public static async Task<MusicLibraryService<TFile, TFileFactory>> GetService()
        {
            var type = typeof(MusicLibraryService<TFile, TFileFactory>);
            type.LogByType("获取服务实例");
            if (_fileTypeFilter is null)
            {
                var ex = new Exception("Filter has not set up. Please use 'SetupFileTypeFilter' method to set filter first");
                type.LogByType(ex);
                throw ex;
            }

            if (Current is null)
                Current = new MusicLibraryService<TFile, TFileFactory>(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music), _fileTypeFilter);
            return Current;
        }
    }
}
