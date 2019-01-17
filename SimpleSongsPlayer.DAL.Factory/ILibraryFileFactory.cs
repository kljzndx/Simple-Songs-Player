using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public interface ILibraryFileFactory<TFile> where TFile : ILibraryFile
    {
        Task<TFile> FromStorageFile(IStorageFolder libraryFolder, StorageFile file, string dbVersion);
        Task<TFile> FromFilePath(IStorageFolder libraryFolder, string path, string dbVersion);
    }
}