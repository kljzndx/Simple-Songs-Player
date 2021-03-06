﻿using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Log.Models;

namespace SimpleSongsPlayer.Test.Service
{
    [TestClass]
    public class LoggerServiceTest
    {
        [TestMethod]
        public void GetLogger()
        {
            var logger = LoggerService.GetLogger(LoggerMembers.UnitTest);
            Assert.IsNotNull(logger);
        }

        [TestMethod]
        public async Task ReadLogs()
        {
            var logger = LoggerService.GetLogger(LoggerMembers.UnitTest);
            logger.Info("测试1");
            logger.Info("测试2");
            logger.Info("测试3");
            logger.Info("测试4");
            logger.Info("测试5");
            var text = await LoggerService.ReadLogs(LoggerMembers.UnitTest, 5);
            Assert.IsFalse(String.IsNullOrWhiteSpace(text));
        }
    }
}