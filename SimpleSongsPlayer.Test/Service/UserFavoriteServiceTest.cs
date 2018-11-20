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
            var FavoriteService = UserFavoriteService.GetService(libraryService);
            List<MusicFile> allFiles = libraryService.GetFiles();
            FavoriteService.AddRange("test", allFiles);
            List<IGrouping<string, MusicFile>> f = FavoriteService.GetFiles();
            Assert.IsTrue(f.Count > 0);
        }

        [TestMethod]
        public async Task RemoveRange()
        {
            var libraryService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();
            var FavoriteService = UserFavoriteService.GetService(libraryService);
            List<MusicFile> allFiles = libraryService.GetFiles();
            FavoriteService.RemoveRange("test", allFiles);
            List<IGrouping<string, MusicFile>> f = FavoriteService.GetFiles();
            Assert.IsFalse(f.Any());
        }
    }
}