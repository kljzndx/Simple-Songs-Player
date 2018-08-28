using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;
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

            List<StorageFile> allFiles = new List<StorageFile>();
            foreach (var folder in folders.ToList())
            {
                var files = await folder.GetFilesAsync(CommonFileQuery.OrderByName);
                allFiles.AddRange(files);
            }

            var songFiles = MusicFileScanner.ScanFiles(allFiles);
            var lyricsFiles = LyricsFileScanner.ScanFiles(allFiles);

            foreach (var songFile in songFiles)
                AllSongs.Add(await Song.CreateFromStorageFile(songFile));

            foreach (var lyricsFile in lyricsFiles)
                AllLyricBlocks.Add(new LyricBlock(lyricsFile.DisplayName, await FileReader.ReadFileAsync(lyricsFile)));
        }
    }
}