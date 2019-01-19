using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSTest.Extensions.Contracts;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.Test.Service
{
    [TestClass]
    public class LyricIndexServiceTest
    {
        [ContractTestCase]
        public void ScanFiles()
        {
            var mService = new TestMusicService();
            var lService = new TestLyricService();
            var indexService = LyricIndexService.GetService(mService, lService);

            "正常扫描".Test(async () =>
            {
                await indexService.ScanAsync();
                var indexes = await indexService.GetFiles();
                Assert.IsTrue(indexes.Count == 8);
            });
            "添加文件".Test(async () =>
            {
                mService.TestAdd();
                // Assert.IsTrue((await mService.GetFiles()).Count == 10);

                await indexService.ScanAsync();
                var indexes = await indexService.GetFiles();
                Assert.IsTrue(indexes.Count == 10);
            });
        }

        class TestMusicService : IFileService<MusicFile>
        {
            private List<MusicFile> _source;

            public event EventHandler<IEnumerable<MusicFile>> FilesAdded;
            public event EventHandler<IEnumerable<MusicFile>> FilesRemoved;

            public TestMusicService()
            {
                _source = new List<MusicFile>
                {
                    new MusicFile {FileName = "xxx.mp3", Path = "c:/xxx/xxx.mp3"},
                    new MusicFile {FileName = "xxx.flac", Path = "c:/xxx/xxx.flac"},
                    new MusicFile {FileName = "xxx.aac", Path = "c:/xxx/xxx.aac"},
                    new MusicFile {FileName = "ttt.mp3", Path = "c:/xxx/ttt.mp3"},
                    new MusicFile {FileName = "ggg.mp3", Path = "c:/xxx/ggg.mp3"},
                    new MusicFile {FileName = "ggg.aac", Path = "c:/xxx/ggg.aac"},
                    new MusicFile {FileName = "hhh.m4a", Path = "c:/xxx/hhh.m4a"},
                    new MusicFile {FileName = "yyy.mp3", Path = "c:/xxx/yyy.mp3"}
                };
            }

            public Task<List<MusicFile>> GetFiles()
            {
                return Task.FromResult(_source.ToList());
            }

            public void TestAdd()
            {
                var add = new List<MusicFile>
                {
                    new MusicFile {FileName = "yyy.aac", Path = "c:/xxx/yyy.aac"},
                    new MusicFile {FileName = "yyy.flac", Path = "c:/xxx/yyy.flac"}
                };

                _source.AddRange(add);
                FilesAdded?.Invoke(this, add);
            }

            
        }

        class TestLyricService : IFileService<LyricFile>
        {
            public event EventHandler<IEnumerable<LyricFile>> FilesAdded;
            public event EventHandler<IEnumerable<LyricFile>> FilesRemoved;

            public Task<List<LyricFile>> GetFiles()
            {
                return Task.FromResult(new List<LyricFile>
                {
                    new LyricFile{ FileName = "xxx.lrc", Path = "c:/xxx/xxx.lrc" },
                    new LyricFile{ FileName = "ttt.lrc", Path = "c:/xxx/ttt.lrc" },
                    new LyricFile{ FileName = "ggg.lrc", Path = "c:/xxx/ggg.lrc" },
                    new LyricFile{ FileName = "hhh.lrc", Path = "c:/xxx/hhh.lrc" },
                    new LyricFile{ FileName = "yyy.lrc", Path = "c:/xxx/yyy.lrc" }
                });
            }
        }
    }
}