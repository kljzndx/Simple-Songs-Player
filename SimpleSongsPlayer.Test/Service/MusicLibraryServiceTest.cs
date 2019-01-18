using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.Test.Service
{
    [TestClass]
    public class MusicLibraryServiceTest
    {
        [TestMethod]
        public async Task Scan()
        {
            var service = (await MusicLibraryFileServiceManager.GetManager()).GetMusicFileService();
            await service.ScanFiles();
            var files = await service.GetFiles();
            Assert.IsTrue(files.Any());
        }

        [TestMethod]
        public async Task GetFiles()
        {
            var service = (await MusicLibraryFileServiceManager.GetManager()).GetMusicFileService();
            var files = await service.GetFiles();
            Assert.IsTrue(files.Count > 20);
        }
    }
}
