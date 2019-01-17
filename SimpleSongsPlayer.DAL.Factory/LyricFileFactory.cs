using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public class LyricFileFactory : ILibraryFileFactory<LyricFile>
    {
        public async Task<LyricFile> FromStorageFile(IStorageFolder libraryFolder, StorageFile file, string dbVersion)
        {
            var prop = await file.GetBasicPropertiesAsync();
            var paths = file.Path.Split('\\').ToList();
            paths.Remove(paths.Last());
            string parent = String.Join("\\", paths);

            return new LyricFile
            {
                FileName = file.Name,
                LibraryFolder = libraryFolder.Path,
                ParentFolder = parent,
                Path = file.Path,
                ChangeDate = prop.DateModified.DateTime,
                DBVersion = dbVersion
            };
        }

        public async Task<LyricFile> FromFilePath(IStorageFolder libraryFolder, string path, string dbVersion) =>
            await FromStorageFile(libraryFolder, await StorageFile.GetFileFromPathAsync(path), dbVersion);
    }
}