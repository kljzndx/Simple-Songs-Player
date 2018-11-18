﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

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

                if (addFiles.Any())
                    AddFileRange(addFiles);

                return;
            }

            foreach (var libraryGroups in musicFiles.GroupBy(f => f.LibraryFolder))
                if (musicLibrary.Folders.All(d => d.Name != libraryGroups.Key))
                    RemoveRange(libraryGroups);

            var allGroup = musicFiles.GroupBy(f => f.LibraryFolder).ToDictionary(g => g.Key, g => g.ToList());

            foreach (var folder in musicLibrary.Folders)
            {
                var myFiles = allGroup[folder.Name];
                var myFilePaths = myFiles.Select(f => f.Path).ToList();

                var systemFiles = await folder.CreateFileQueryWithOptions(scanOptions).GetFilesAsync();
                var systemFilePaths = systemFiles.Select(f => f.Path).ToList();

                // 清理垃圾
                var needRemoveFilePaths = myFilePaths.Where(mf => !systemFilePaths.Contains(mf)).ToList();
                removeFiles.AddRange(myFiles.Where(f => needRemoveFilePaths.Contains(f.Path)));

                // 查缺补漏
                var needAddFiles = systemFiles.Where(sf => !myFilePaths.Contains(sf.Path));
                foreach (var filePath in needAddFiles)
                    addFiles.Add(await fileFactory.FromStorageFile(folder.Name, filePath));
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
            FilesAdded?.Invoke(this, filesList);
        }

        private void RemoveRange(IEnumerable<TFile> files)
        {
            List<TFile> filesList = new List<TFile>(files);

            helper.RemoveRange(filesList);
            musicFiles.RemoveAll(filesList.Contains);
            FilesRemoved?.Invoke(this, filesList);
        }

        public static void SetupFileTypeFilter(params string[] fileTypeFilter)
        {
            if (_fileTypeFilter is null)
                _fileTypeFilter = fileTypeFilter;
            else 
                throw new Exception("Filter has been set up");
        }

        public static async Task<MusicLibraryService<TFile, TFileFactory>> GetService()
        {
            if (_fileTypeFilter is null)
                throw new Exception("Filter has not set up. Please use 'SetupFileTypeFilter' method to set filter");

            if (Current is null)
                Current = new MusicLibraryService<TFile, TFileFactory>(await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music), _fileTypeFilter);
            return Current;
        }
    }
}
