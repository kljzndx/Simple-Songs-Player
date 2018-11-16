using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.Test.Service
{
    [TestClass]
    public class MusicLibraryServiceTest
    {
        [TestMethod]
        public async Task Scan()
        {
            var service = await MusicLibraryService.GetService();
            await service.ScanFiles();
            var files = service.GetFiles();
            Assert.IsTrue(files.Any());
        }
    }
}
