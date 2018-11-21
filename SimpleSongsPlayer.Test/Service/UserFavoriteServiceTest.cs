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

        static UserFavoriteServiceTest()
        {
            MusicLibraryService<MusicFile, MusicFileFactory>.SetupFileTypeFilter("mp3", "aac", "wav", "flac", "alac", "m4a");
        }

        [TestMethod]
        public async Task AddRange()
        {
            var libraryService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();
            var favoriteService = UserFavoriteService.GetService(libraryService);
            List<MusicFile> allFiles = libraryService.GetFiles();
            favoriteService.AddRange("test", allFiles);
            List<IGrouping<string, MusicFile>> f = favoriteService.GetFiles();
            Assert.IsTrue(f.Count > 0);
        }

        [TestMethod]
        public async Task RenameGroup()
        {
            var libraryService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();
            var favoriteService = UserFavoriteService.GetService(libraryService);
            favoriteService.RenameGroup("test", "newTest");
            List<IGrouping<string, MusicFile>> f = favoriteService.GetFiles();
            Assert.IsTrue(f.All(uf => uf.Key != "test") && f.Any(uf => uf.Key == "newTest"));
        }

        [TestMethod]
        public async Task RemoveRange()
        {
            var libraryService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();
            var favoriteService = UserFavoriteService.GetService(libraryService);
            List<MusicFile> allFiles = libraryService.GetFiles();
            favoriteService.RemoveRange("newTest", allFiles);
            List<IGrouping<string, MusicFile>> f = favoriteService.GetFiles();
            Assert.IsFalse(f.Any());
        }
    }
}