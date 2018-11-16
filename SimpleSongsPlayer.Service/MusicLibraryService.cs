using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class MusicLibraryService<TFile, TFileFactory> where TFile : class, ILibraryFile where TFileFactory : ILibraryFileFactory<TFile>, new()
    {
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
            musicLibrary = library;
            musicFiles = helper.ToList();

            foreach (var filter in fileTypeFilter)
                scanOptions.FileTypeFilter.Add(filter[0] == '.' ? filter : $".{filter}");
        }

        public List<TFile> GetFiles()
        {
            return musicFiles.ToList();
        }

        public async Task ScanFiles()
        {
            List<TFile> addFiles = new List<TFile>();
            List<TFile> removeFiles = new List<TFile>();

            if (!musicFiles.Any())
            {
                foreach (var folder in musicLibrary.Folders)
                {
                    var allFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                    foreach (var file in allFiles)
                        addFiles.Add(await fileFactory.FromStorageFile(folder.Name, file));
                }

                AddFileRange(addFiles);
                return;
            }

            foreach (var folderName in musicFiles.Select(f => f.LibraryFolder).Distinct())
                if (musicLibrary.Folders.All(d => d.Name != folderName))
                    RemoveRange(musicFiles.Where(f => f.LibraryFolder == folderName));

            var allGroup = musicFiles.GroupBy(f => f.LibraryFolder).ToList();

            foreach (var folder in musicLibrary.Folders)
            {
                var myFiles = allGroup.FirstOrDefault(g => g.Key == folder.Name)?.ToList();
                var myFilePaths = myFiles?.Select(f => f.Path).ToList();

                var systemFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                var systemFilePaths = systemFiles.Select(f => f.Path).ToList();

                // 清理垃圾
                if (myFilePaths != null)
                {
                    var needRemoveFilePaths = myFilePaths.Where(mf => !systemFilePaths.Contains(mf)).ToList();
                    removeFiles.AddRange(myFiles.Where(f => needRemoveFilePaths.Contains(f.Path)));
                }

                // 查缺补漏
                var needAddFiles = systemFiles.Where(sf => myFilePaths is null || !myFilePaths.Contains(sf.Path));
                foreach (var file in needAddFiles)
                    addFiles.Add(await fileFactory.FromStorageFile(folder.Name, file));
            }

            if (addFiles.Any())
                AddFileRange(addFiles);

            if (removeFiles.Any())
                RemoveRange(removeFiles);
        }

        private void AddFileRange(IEnumerable<TFile> files)
        {
            List<TFile> filesList = new List<TFile>(files);

            helper.AddRange(filesList);
            musicFiles.AddRange(filesList);
            FilesAdded?.Invoke(null, filesList);
        }

        private void RemoveRange(IEnumerable<TFile> files)
        {
            List<TFile> filesList = new List<TFile>(files);

            helper.RemoveRange(filesList);
            musicFiles.RemoveAll(filesList.Contains);
            FilesRemoved?.Invoke(null, filesList);
        }

        public static async Task<MusicLibraryService<TFile, TFileFactory>> GetService(params string[] fileTypeFilter)
        {
            if (Current is null)
                Current = new MusicLibraryService<TFile, TFileFactory>(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music), fileTypeFilter);
            return Current;
        }
    }
}
