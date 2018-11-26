using System;
using System.Threading.Tasks;
using Windows.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.Test.DAL
{
    [TestClass]
    public class ContextHelperTest
    {
        private readonly ContextHelper<FilesContext, MusicFile> helper = new ContextHelper<FilesContext, MusicFile>();

        public MusicFile GetFile()
        {
            MusicFile file = new MusicFile
            {
                LibraryFolder = "Music", Title = "Journey", Duration = TimeSpan.FromMinutes(3),
                Path = "C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3"
            };

            return file;
        }

        [TestMethod]
        public async Task Add()
        {
            var queryResult = await helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            if (queryResult != null)
            {
                Assert.IsNotNull(queryResult);
                return;
            }

            var file = GetFile();

            await helper.Add(file);
            queryResult = await helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            Assert.IsNotNull(queryResult);
        }

        [TestMethod]
        public async Task ToList()
        {
            var result = await helper.ToList();
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public async Task Remove()
        {
            var obj = GetFile();

            await helper.Remove(obj);

            var query = await helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            Assert.IsNull(query);
        }
    }
}
