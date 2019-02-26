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
            var mService = TestMusicService.Current;
            var lService = TestLyricService.Current;
            var indexService = LyricIndexService.GetService(mService, lService);

            "正常扫描".Test(async () =>
            {
                await indexService.ScanAsync();
                var indexes = await indexService.GetData();
                Assert.IsTrue(indexes.Count == 8);
            });
            "添加文件".Test(async () =>
            {
                mService.TestAdd();
                // Assert.IsTrue((await mService.GetData()).Count == 10);

                await indexService.ScanAsync();
                var indexes = await indexService.GetData();
                Assert.IsTrue(indexes.Count == 10);
            });
        }

        class TestMusicService : IDataService<MusicFile>
        {
            public static readonly TestMusicService Current = new TestMusicService();

            private List<MusicFile> _source;

            public event EventHandler<IEnumerable<MusicFile>> DataAdded;
            public event EventHandler<IEnumerable<MusicFile>> DataRemoved;

            private TestMusicService()
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

            public Task<List<MusicFile>> GetData()
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
                DataAdded?.Invoke(this, add);
            }

            
        }

        class TestLyricService : IDataService<LyricFile>
        {
            public static readonly TestLyricService Current = new TestLyricService();

            private readonly List<LyricFile> _source;

            public event EventHandler<IEnumerable<LyricFile>> DataAdded;
            public event EventHandler<IEnumerable<LyricFile>> DataRemoved;

            private TestLyricService()
            {
                _source = new List<LyricFile>
                {
                    new LyricFile {FileName = "xxx.lrc", Path = "c:/xxx/xxx.lrc"},
                    new LyricFile {FileName = "ttt.lrc", Path = "c:/xxx/ttt.lrc"},
                    new LyricFile {FileName = "ggg.lrc", Path = "c:/xxx/ggg.lrc"},
                    new LyricFile {FileName = "hhh.lrc", Path = "c:/xxx/hhh.lrc"},
                    new LyricFile {FileName = "yyy.lrc", Path = "c:/xxx/yyy.lrc"}
                };
            }

            public Task<List<LyricFile>> GetData()
            {
                return Task.FromResult(_source);
            }

            public void TestChange()
            {
                _source[1].Path = "c:/xxx/ddd/ttt.lrc";
            }
        }
    }
}