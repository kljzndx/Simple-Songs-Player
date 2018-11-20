using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class MusicFileFactory : ILibraryFileFactory<MusicFile>
    {
        public async Task<MusicFile> FromStorageFile(string libraryFolder, StorageFile file)
        {
            var properties = await file.Properties.GetMusicPropertiesAsync();

            return new MusicFile
            {
                Title = String.IsNullOrWhiteSpace(properties.Title) ? file.DisplayName : properties.Title,
                Artist = properties.Artist,
                Album = properties.Album,
                Path = file.Path,
                Duration = properties.Duration,
                LibraryFolder = libraryFolder
            };
        }

        public async Task<MusicFile> FromFilePath(string path)
        {
            var file = await StorageFile.GetFileFromPathAsync(path);
            var properties = await file.Properties.GetMusicPropertiesAsync();

            return new MusicFile
            {
                Title = String.IsNullOrWhiteSpace(properties.Title) ? file.DisplayName : properties.Title,
                Artist = properties.Artist,
                Album = properties.Album,
                Path = file.Path,
                Duration = properties.Duration,
                LibraryFolder = ""
            };
        }
    }
}