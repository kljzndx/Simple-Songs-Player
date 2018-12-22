using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class MusicFileFactory : ILibraryFileFactory<MusicFile>
    {
        public async Task<MusicFile> FromStorageFile(string libraryFolder, StorageFile file)
        {
            var basicProperties = await file.GetBasicPropertiesAsync();
            var musicProperties = await file.Properties.GetMusicPropertiesAsync();

            return new MusicFile
            {
                FileName = file.Name,
                Title = String.IsNullOrWhiteSpace(musicProperties.Title) ? file.DisplayName : musicProperties.Title,
                Artist = musicProperties.Artist,
                Album = musicProperties.Album,
                Path = file.Path,
                Duration = musicProperties.Duration,
                LibraryFolder = libraryFolder,
                ChangeDate = basicProperties.DateModified.DateTime
            };
        }

        public async Task<MusicFile> FromFilePath(string libraryFolder, string path) => await FromStorageFile(libraryFolder, await StorageFile.GetFileFromPathAsync(path));
    }
}