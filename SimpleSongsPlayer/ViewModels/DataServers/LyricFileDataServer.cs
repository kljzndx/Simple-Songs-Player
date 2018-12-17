using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public class LyricFileDataServer
    {
        public static readonly LyricFileDataServer Current = new LyricFileDataServer();

        private MusicLibraryService<LyricFile, LyricFileFactory> service;

        private LyricFileDataServer()
        {
        }

        public ObservableCollection<LyricFileDTO> Data { get; } = new ObservableCollection<LyricFileDTO>();

        public async Task Init()
        {
            service = await MusicLibraryService<LyricFile, LyricFileFactory>.GetService();
            var files = await service.GetFiles();
            foreach (var file in files)
                Data.Add(new LyricFileDTO(file));

            service.FilesAdded += Service_FilesAdded;
            service.FilesRemoved += Service_FilesRemoved;
        }

        public async Task ScanFile()
        {
            if (service is null)
                await Init();

            await service.ScanFiles();
        }

        private void Service_FilesAdded(object sender, IEnumerable<LyricFile> e)
        {
            foreach (var file in e)
                Data.Add(new LyricFileDTO(file));
        }

        private void Service_FilesRemoved(object sender, IEnumerable<LyricFile> e)
        {
            foreach (var file in e)
                Data.Remove(Data.First(d => d.FilePath == file.Path));
        }
    }
}