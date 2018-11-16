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
            var service = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService("mp3", "aac", "wav", "flac", "alac", "m4a");
            await service.ScanFiles();
            var files = service.GetFiles();
            Assert.IsTrue(files.Any());
        }

        [TestMethod]
        public async Task GetFiles()
        {
            var service = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService("mp3", "aac", "wav", "flac", "alac", "m4a");
            var files = service.GetFiles();
            Assert.IsTrue(files.Count > 20);
        }
    }
}
