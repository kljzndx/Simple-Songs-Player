﻿using System;
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
        public void Test_Add()
        {
            var queryResult = helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            if (queryResult != null)
            {
                Assert.IsNotNull(queryResult);
                return;
            }

            var file = GetFile();

            helper.Add(file);
            queryResult = helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            Assert.IsNotNull(queryResult);
        }

        [TestMethod]
        public void Test_Remove()
        {
            var obj = GetFile();

            helper.Remove(obj);

            var query = helper.Find("C:\\Users\\kljzn\\Music\\Capo Productions - Journey.mp3");
            Assert.IsNull(query);
        }

        [TestMethod]
        public void Test_ToList()
        {
            var result = helper.ToList();
            Assert.IsTrue(result.Count > 20);
        }
    }
}