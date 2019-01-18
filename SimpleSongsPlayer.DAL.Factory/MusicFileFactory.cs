﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class MusicFileFactory : ILibraryFileFactory<MusicFile>
    {
        public async Task<MusicFile> FromStorageFile(string libraryFolder, StorageFile file, string dbVersion)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();
            var musicProperties = await file.Properties.GetMusicPropertiesAsync();
            var paths = file.Path.Split('\\').ToList();
            paths.Remove(paths.Last());
            string parent = String.Join("\\", paths);

            return new MusicFile
            {
                FileName = file.Name,
                Title = String.IsNullOrWhiteSpace(musicProperties.Title) ? file.DisplayName : musicProperties.Title,
                Artist = musicProperties.Artist,
                Album = musicProperties.Album,
                ParentFolder = parent,
                Path = file.Path,
                Duration = musicProperties.Duration,
                LibraryFolder = libraryFolder,
                ChangeDate = basicProperties.DateModified.DateTime,
                DBVersion = dbVersion
            };
        }

        public async Task<MusicFile> FromFilePath(string libraryFolder, string path, string dbVersion) =>
            await FromStorageFile(libraryFolder, await StorageFile.GetFileFromPathAsync(path), dbVersion);
    }
}