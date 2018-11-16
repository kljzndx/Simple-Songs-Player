using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class MusicLibraryService
    {
        private static MusicLibraryService Current;

        private readonly QueryOptions scanOptions = new QueryOptions();
        private readonly StorageLibrary musicLibrary;
        private readonly ContextHelper<FilesContext, MusicFile> helper = new ContextHelper<FilesContext, MusicFile>();
        private readonly List<MusicFile> musicFiles;

        public event EventHandler<IEnumerable<MusicFile>> FilesAdded;
        public event EventHandler<IEnumerable<MusicFile>> FilesRemoved;

        private MusicLibraryService(StorageLibrary library)
        {
            musicLibrary = library;
            musicFiles = helper.ToList();

            scanOptions.FileTypeFilter.Add(".mp3");
            scanOptions.FileTypeFilter.Add(".aac");
            scanOptions.FileTypeFilter.Add(".wav");
            scanOptions.FileTypeFilter.Add(".flac");
            scanOptions.FileTypeFilter.Add(".alac");
            scanOptions.FileTypeFilter.Add(".m4a");
        }

        public List<MusicFile> GetFiles()
        {
            return musicFiles.ToList();
        }

        public async Task ScanFiles()
        {
            List<MusicFile> addFiles = new List<MusicFile>();
            List<MusicFile> removeFiles = new List<MusicFile>();

            if (!musicFiles.Any())
            {
                foreach (var folder in musicLibrary.Folders)
                {
                    var allFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                    foreach (var file in allFiles)
                        addFiles.Add(await MusicFileFactory.FromStorageFile(folder.Name, file));
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
                    addFiles.Add(await MusicFileFactory.FromStorageFile(folder.Name, file));
            }

            if (addFiles.Any())
                AddFileRange(addFiles);

            if (removeFiles.Any())
                RemoveRange(removeFiles);
        }

        private void AddFileRange(IEnumerable<MusicFile> files)
        {
            List<MusicFile> filesList = new List<MusicFile>(files);

            helper.AddRange(filesList);
            musicFiles.AddRange(filesList);
            FilesAdded?.Invoke(null, filesList);
        }

        private void RemoveRange(IEnumerable<MusicFile> files)
        {
            List<MusicFile> filesList = new List<MusicFile>(files);

            helper.RemoveRange(filesList);
            musicFiles.RemoveAll(filesList.Contains);
            FilesRemoved?.Invoke(null, filesList);
        }

        public static async Task<MusicLibraryService> GetService()
        {
            if (Current is null)
                Current = new MusicLibraryService(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music));
            return Current;
        }
    }
}
