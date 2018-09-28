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
            AllSongs = new List<Song>();
            AllLyricBlocks = new List<LyricBlock>();

            foreach (var folder in folders.ToList())
            {
                var songFiles = await MusicFileScanner.ScanFiles(folder);
                var lyricsFiles = await LyricsFileScanner.ScanFiles(folder);

                foreach (var songFile in songFiles)
                    AllSongs.Add(await Song.CreateFromStorageFile(songFile));

                foreach (var lyricsFile in lyricsFiles)
                {
                    try
                    {
                        var block = new LyricBlock(lyricsFile.DisplayName, await FileReader.ReadFileAsync(lyricsFile));
                        AllLyricBlocks.Add(block);
                    }
                    catch (LyricsParsingFailedException e)
                    {
                        MessageBox.ShowAsync(e.Message, App.MessageBoxResourceLoader.GetString("Close"));
                        continue;
                    }
                }
            }
        }
    }
}