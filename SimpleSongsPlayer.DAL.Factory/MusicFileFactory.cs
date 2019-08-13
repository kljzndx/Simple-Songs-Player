using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class MusicFileFactory : ILibraryFileFactory<MusicFile>
    {
        public async Task<MusicFile> FromStorageFile(string libraryFolder, StorageFile file, string dbVersion)
        {
            var basicProperties = await file.GetProperties(async f => await f.GetBasicPropertiesAsync());
            var musicProperties = await file.GetProperties(async f => await f.Properties.GetMusicPropertiesAsync());

            return new MusicFile
            {
                FileName = file.Name,
                LibraryFolder = libraryFolder,
                ChangeDate = basicProperties.DateModified.DateTime,
                ParentFolder = file.Path.TakeParentPath(),
                Path = file.Path,

                TrackNumber = musicProperties.TrackNumber,
                Title = String.IsNullOrWhiteSpace(musicProperties.Title) ? file.DisplayName : musicProperties.Title,
                Artist = musicProperties.Artist,
                Album = musicProperties.Album,
                Duration = musicProperties.Duration,

                DBVersion = dbVersion
            };
        }

        public async Task<MusicFile> FromFilePath(string libraryFolder, string path, string dbVersion) =>
            await FromStorageFile(libraryFolder, await StorageFile.GetFileFromPathAsync(path), dbVersion);
    }
}