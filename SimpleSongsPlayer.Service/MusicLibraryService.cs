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
        private static readonly string Where;

        private static string[] _fileTypeFilter;
        private static MusicLibraryService<TFile, TFileFactory> Current;
        private static readonly Logger Logger = LoggerService.GetLogger(LoggerMembers.Service);

        private readonly QueryOptions scanOptions = new QueryOptions();
        private readonly StorageLibrary musicLibrary;
        private readonly ContextHelper<FilesContext, TFile> helper = new ContextHelper<FilesContext, TFile>();
        private readonly TFileFactory fileFactory = new TFileFactory();
        private readonly List<TFile> musicFiles;

        public event EventHandler<IEnumerable<TFile>> FilesAdded;
        public event EventHandler<IEnumerable<TFile>> FilesRemoved;

        static MusicLibraryService()
        {
            Where = $"in MusicLibraryService< {typeof(TFile).Name} >";
        }

        private MusicLibraryService(StorageLibrary library, string[] fileTypeFilter)
        {
            Log("正在构造服务，正在获取音乐库引用");
            musicLibrary = library;
            Log("正在获取文件");
            musicFiles = helper.ToList();

            Log("正在配置文件筛选器");
            foreach (var filter in fileTypeFilter)
                scanOptions.FileTypeFilter.Add(filter[0] == '.' ? filter : $".{filter}");

            Log("构造完成");
        }

        public List<TFile> GetFiles()
        {
            Log("正在输出文件列表");
            return musicFiles.ToList();
        }

        public async Task ScanFiles()
        {
            Log("准备 ‘操作集合’");
            List<TFile> addFiles = new List<TFile>();
            List<TFile> removeFiles = new List<TFile>();

            Log("检测数据库是否有数据");
            if (!musicFiles.Any())
            {
                Log("检测到数据库里没数据，进入收集模式，开始扫描音乐库");
                foreach (var folder in musicLibrary.Folders)
                {
                    var allFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                    foreach (var file in allFiles)
                        addFiles.Add(await fileFactory.FromStorageFile(folder.Name, file));
                }

                Log("应用 ‘添加操作集合’");
                if (addFiles.Any())
                    AddFileRange(addFiles);

                Log("完成 ‘添加操作集合’ 应用");
                return;
            }

            Log("检测到数据库里有数据，进入 ‘对比’ 模式，开始检测音乐库是否有文件夹被移除");
            foreach (var libraryGroups in musicFiles.GroupBy(f => f.LibraryFolder))
                if (musicLibrary.Folders.All(d => d.Name != libraryGroups.Key))
                {
                    Log($"已检测到有文件夹已被移除，正在同步至数据库");
                    RemoveRange(libraryGroups);
                    Log("完成同步");
                }

            Log("将数据库里的数据分类");
            var allGroup = musicFiles.GroupBy(f => f.LibraryFolder).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var folder in musicLibrary.Folders)
            {
                Log("从数据库获取对应当前文件夹的数据");
                List<TFile> myFiles;
                bool isGetFiles = allGroup.TryGetValue(folder.Name, out myFiles);
                var myFilePaths = myFiles?.Select(f => f.Path).ToList();

                Log("从音乐库获取对应当前文件夹的数据");
                var systemFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                var systemFilePaths = systemFiles.Select(f => f.Path).ToList();

                Log("进入对比流程");
                if (isGetFiles)
                {
                    Log("获取需要移除的文件");
                    var needRemoveFilePaths = myFilePaths.Where(mf => !systemFilePaths.Contains(mf)).ToList();
                    Log("将结果添加至 ‘移除操作’ 集合");
                    removeFiles.AddRange(myFiles.Where(f => needRemoveFilePaths.Contains(f.Path)));
                }

                Log("获取需要添加的文件");
                var needAddFiles = systemFiles.Where(sf => !isGetFiles || !myFilePaths.Contains(sf.Path));
                Log("将结果添加至 ‘添加操作’ 集合");
                foreach (var filePath in needAddFiles)
                    addFiles.Add(await fileFactory.FromStorageFile(folder.Name, filePath));
            }

            Log("应用 ‘操作集合’");
            if (addFiles.Any())
                AddFileRange(addFiles);

            if (removeFiles.Any())
                RemoveRange(removeFiles);

            Log("完成文件扫描");
        }

        private void AddFileRange(IEnumerable<TFile> files)
        {
            Log("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);

            Log("正在添加到数据库");
            helper.AddRange(filesList);
            Log("正在添加到列表");
            musicFiles.AddRange(filesList);
            Log("触发 ‘添加完成’ 事件");
            FilesAdded?.Invoke(this, filesList);
        }

        private void RemoveRange(IEnumerable<TFile> files)
        {
            Log("正在接受数据");
            List<TFile> filesList = new List<TFile>(files);

            Log("正在从数据库移除");
            helper.RemoveRange(filesList);
            Log("正在从列表移除");
            musicFiles.RemoveAll(filesList.Contains);
            Log("触发 ‘移除完成’ 事件");
            FilesRemoved?.Invoke(this, filesList);
        }

        public static void SetupFileTypeFilter(params string[] fileTypeFilter)
        {
            Log("设置文件筛选器");
            if (_fileTypeFilter is null)
                _fileTypeFilter = fileTypeFilter;
            else
                Log("已设置筛选器");
        }

        public static async Task<MusicLibraryService<TFile, TFileFactory>> GetService()
        {
            Log("获取服务实例");
            if (_fileTypeFilter is null)
            {
                var ex = new Exception("Filter has not set up. Please use 'SetupFileTypeFilter' method to set filter first");
                Log(ex);
                throw ex;
            }

            if (Current is null)
                Current = new MusicLibraryService<TFile, TFileFactory>(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music), _fileTypeFilter);
            return Current;
        }

        private static void Log(string message)
        {
            Logger.Info($"{message} {Where}");
        }
        
        private static void Log(Exception exception, string message = "")
        {
            Logger.Error(exception, $"{message} {Where}");
        }

    }
}
