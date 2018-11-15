using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.Test.DAL
{
    [TestClass]
    public class ContextHelper
    {
        [TestMethod]
        public void Add()
        {
            MusicFile file = new MusicFile
            {
                LibraryFolder = "Music", Title = "Journey", Duration = TimeSpan.FromMinutes(3),
                Path = "C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3"
            };

            ContextHelper<FilesContext, MusicFile>.Add(file);

            Assert.IsNotNull(ContextHelper<FilesContext, MusicFile>.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3"));
        }
    }
}
