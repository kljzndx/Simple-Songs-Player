using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using GalaSoft.MvvmLight;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.DataModel.Exceptions;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Operator;
using SimpleSongsPlayer.Operator.FileScanners;

namespace SimpleSongsPlayer.ViewModels
{
    public class LoadingViewModel : ViewModelBase
    {
        private static readonly MusicFileScanner MusicFileScanner = new MusicFileScanner();
        private static readonly LyricsFileScanner LyricsFileScanner = new LyricsFileScanner();

        public List<Song> AllSongs { get; private set; }
        public List<LyricBlock> AllLyricBlocks { get; private set; }

        public async Task ScanFolders(IEnumerable<StorageFolder> folders)
        {
            LoggerMembers.VmLogger.Info("正在准备列表");
            AllSongs = new List<Song>();
            AllLyricBlocks = new List<LyricBlock>();

            LoggerMembers.VmLogger.Info("开始扫描文件夹");
            foreach (var folder in folders.ToList())
            {
                LoggerMembers.VmLogger.Info("正在获取当前文件夹下的所有歌曲文件");
                var songFiles = await MusicFileScanner.ScanFiles(folder);
                LoggerMembers.VmLogger.Info("正在获取当前文件夹下的所有歌词文件");
                var lyricsFiles = await LyricsFileScanner.ScanFiles(folder);

                LoggerMembers.VmLogger.Info("完成文件获取， 开始解析歌曲文件");
                foreach (var songFile in songFiles)
                    AllSongs.Add(await Song.CreateFromStorageFile(songFile));

                LoggerMembers.VmLogger.Info("完成歌曲文件解析， 开始解析歌词文件");
                foreach (var lyricsFile in lyricsFiles)
                {
                    try
                    {
                        var block = new LyricBlock(lyricsFile.DisplayName, await FileReader.ReadFileAsync(lyricsFile));
                        AllLyricBlocks.Add(block);
                    }
                    catch (LyricsParsingFailedException e)
                    {
                        LoggerMembers.VmLogger.Error(e);
                        MessageBox.ShowAsync(e.Message, App.MessageBoxResourceLoader.GetString("Close"));
                        continue;
                    }
                }
                LoggerMembers.VmLogger.Info("已完成所有解析操作");
            }
        }
    }
}