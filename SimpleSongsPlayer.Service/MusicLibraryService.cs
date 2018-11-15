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
    public static class MusicLibraryService
    {
        private static readonly QueryOptions ScanOptions = new QueryOptions();
        private static readonly StorageLibrary MusicLibrary = StorageLibrary.GetLibraryAsync(KnownLibraryId.Music).GetResults();
        private static readonly ContextHelper<FilesContext, MusicFile> Helper = new ContextHelper<FilesContext, MusicFile>();
        private static readonly List<MusicFile> MusicFiles = Helper.ToList();

        public static event EventHandler<IEnumerable<MusicFile>> FilesAdded;
        public static event EventHandler<IEnumerable<MusicFile>> FilesRemoved;

        static MusicLibraryService()
        {
            ScanOptions.FileTypeFilter.Add(".mp3");
            ScanOptions.FileTypeFilter.Add(".aac");
            ScanOptions.FileTypeFilter.Add(".wav");
            ScanOptions.FileTypeFilter.Add(".flac");
            ScanOptions.FileTypeFilter.Add(".alac");
            ScanOptions.FileTypeFilter.Add(".m4a");
        }
        
        public static List<MusicFile> GetFiles()
        {
            return MusicFiles.ToList();
        }

        public static async Task ScanFiles()
        {
            List<MusicFile> addFiles = new List<MusicFile>();
            List<MusicFile> removeFiles = new List<MusicFile>();

            if (!MusicFiles.Any())
            {
                foreach (var folder in MusicLibrary.Folders)
                {
                    var allFiles = await folder.CreateFileQueryWithOptions(ScanOptions).GetFilesAsync();
                    foreach (var file in allFiles)
                        addFiles.Add(await MusicFileFactory.FromStorageFile(folder.Name, file));
                }

                AddFileRange(addFiles);
                return;
            }

            foreach (var folderName in MusicFiles.Select(f => f.LibraryFolder).Distinct())
                if (MusicLibrary.Folders.All(d => d.Name != folderName))
                    RemoveRange(MusicFiles.Where(f => f.LibraryFolder == folderName));

            var allGroup = MusicFiles.GroupBy(f => f.LibraryFolder).ToList();

            foreach (var folder in MusicLibrary.Folders)
            {
                var myFiles = allGroup.FirstOrDefault(g => g.Key == folder.Name)?.ToList();
                var myFilePaths = myFiles?.Select(f => f.Path).ToList();

                var systemFiles = await folder.CreateFileQueryWithOptions(ScanOptions).GetFilesAsync();
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

        private static void AddFileRange(IEnumerable<MusicFile> files)
        {
            List<MusicFile> filesList = new List<MusicFile>(files);

            Helper.AddRange(filesList);
            MusicFiles.AddRange(filesList);
            FilesAdded?.Invoke(null, filesList);
        }

        private static void RemoveRange(IEnumerable<MusicFile> files)
        {
            List<MusicFile> filesList = new List<MusicFile>(files);

            Helper.RemoveRange(filesList);
            MusicFiles.RemoveAll(filesList.Contains);
            FilesRemoved?.Invoke(null, filesList);
        }
    }
}
