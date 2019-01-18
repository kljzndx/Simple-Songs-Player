using System.Threading.Tasks;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public static class MusicLibraryManagerHelper
    {
        public static async Task ScanFiles()
        {
            var _manager = await MusicLibraryFileServiceManager.GetManager();
            await MusicFileDataServer.Current.InitializeMusicService();
            await LyricFileDataServer.Current.Init();
            await _manager.ScanFiles();
        }
    }
}