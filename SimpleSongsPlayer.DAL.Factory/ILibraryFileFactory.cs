using System.Threading.Tasks;
using Windows.Storage;

namespace SimpleSongsPlayer.DAL.Factory
{
    public interface ILibraryFileFactory<TFile> where TFile : ILibraryFile
    {
        Task<TFile> FromStorageFile(string libraryFolder, StorageFile file, string dbVersion);
        Task<TFile> FromFilePath(string libraryFolder, string path, string dbVersion);
    }
}