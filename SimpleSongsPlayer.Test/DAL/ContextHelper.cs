using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.Test.DAL
{
    [TestClass]
    public class ContextHelper
    {
        private readonly ContextHelper<FilesContext, MusicFile> helper = new ContextHelper<FilesContext, MusicFile>();

        public MusicFile Add()
        {
            var queryResult = helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            if (queryResult != null)
                return queryResult;

            MusicFile file = new MusicFile
            {
                LibraryFolder = "Music", Title = "Journey", Duration = TimeSpan.FromMinutes(3),
                Path = "C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3"
            };

            helper.Add(file);
            return file;
        }

        [TestMethod]
        public void Test_Add()
        {
            Add();
            var queryResult = helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            Assert.IsNotNull(queryResult);
        }

        [TestMethod]
        public void Test_Remove()
        {
            var obj = Add();

            helper.Remove(obj);

            var query = helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            Assert.IsNull(query);
        }
    }
}
