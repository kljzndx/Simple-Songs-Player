using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class LyricFileFactory : ILibraryFileFactory<LyricFile>
    {
        public async Task<LyricFile> FromStorageFile(string libraryFolder, StorageFile file)
        {
            var prop = await file.GetBasicPropertiesAsync();
            return new LyricFile
            {
                FileName = file.DisplayName,
                LibraryFolder = libraryFolder,
                Path = file.Path,
                ChangeDate = prop.DateModified.DateTime
            };
        }

        public async Task<LyricFile> FromFilePath(string libraryFolder, string path) =>
            await FromStorageFile(libraryFolder, await StorageFile.GetFileFromPathAsync(path));
    }
}