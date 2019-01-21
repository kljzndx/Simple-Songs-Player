using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.Test.Service
{
    [TestClass]
    public class UserFavoriteServiceTest
    {
        [TestMethod]
        public async Task AddRange()
        {
            var libraryService = await MusicLibraryFileServiceManager.Current.GetMusicFileService();
            var favoriteService = UserFavoriteService.GetService(libraryService);
            List<MusicFile> allFiles = await libraryService.GetFiles();
            await favoriteService.AddRange("test", allFiles.Select(a => a.Path));
            List<IGrouping<string, string>> f = await favoriteService.GetFiles();
            Assert.IsTrue(f.Count > 0);
        }

        [TestMethod]
        public async Task RenameGroup()
        {
            var libraryService = await MusicLibraryFileServiceManager.Current.GetMusicFileService();
            var favoriteService = UserFavoriteService.GetService(libraryService);
            await favoriteService.RenameGroup("test", "newTest");
            List<IGrouping<string, string>> f = await favoriteService.GetFiles();
            Assert.IsTrue(f.All(uf => uf.Key != "test") && f.Any(uf => uf.Key == "newTest"));
        }

        [TestMethod]
        public async Task RemoveRange()
        {
            var libraryService = await MusicLibraryFileServiceManager.Current.GetMusicFileService();
            var favoriteService = UserFavoriteService.GetService(libraryService);
            List<MusicFile> allFiles = await libraryService.GetFiles();
            await favoriteService.RemoveRange("newTest", allFiles.Select(a => a.Path));
            List<IGrouping<string, string>> f = await favoriteService.GetFiles();
            Assert.IsFalse(f.Any());
        }
    }
}