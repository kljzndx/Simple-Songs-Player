using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Log.Models;

namespace SimpleSongsPlayer.Test.Service
{
    [TestClass]
    public class LogExtensionTest
    {
        static LogExtensionTest()
        {
            LogExtension.SetUpAssembly(typeof(App).GetTypeInfo().Assembly, LoggerMembers.UnitTest);
        }

        [TestMethod]
        public async Task LogByObjectTest()
        {
            for (int i = 0; i < 10; i++)
                this.LogByObject($"测试 LogExtension 已测试 {i} 次");

            string str = await LoggerService.ReadLogs(LoggerMembers.UnitTest, 30);
            Assert.IsTrue(str.Contains("测试 LogExtension"));
        }
    }
}