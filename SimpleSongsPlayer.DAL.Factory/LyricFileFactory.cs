using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class LyricFileFactory : ILibraryFileFactory<LyricFile>
    {
        public async Task<LyricFile> FromStorageFile(string libraryFolder, StorageFile file, string dbVersion)
        {
            var prop = await file.GetProperties(async f => await f.GetBasicPropertiesAsync());
            return new LyricFile
            {
                FileName = file.Name,
                LibraryFolder = libraryFolder,
                ParentFolder = file.Path.TakeParentPath(),
                Path = file.Path,
                ChangeDate = prop.DateModified.DateTime,
                DBVersion = dbVersion
            };
        }

        public async Task<LyricFile> FromFilePath(string libraryFolder, string path, string dbVersion) =>
            await FromStorageFile(libraryFolder, await StorageFile.GetFileFromPathAsync(path), dbVersion);
    }
}