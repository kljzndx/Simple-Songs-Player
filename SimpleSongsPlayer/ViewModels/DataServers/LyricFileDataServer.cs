using System;
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
        public event EventHandler<IEnumerable<LyricFileDTO>> FilesAdded;
        public event EventHandler<IEnumerable<LyricFileDTO>> FilesRemoved;

        public async Task Init()
        {
            if (service != null)
                return;

            service = await MusicLibraryService<LyricFile, LyricFileFactory>.GetService();
            var files = await service.GetFiles();
            foreach (var file in files)
                Data.Add(new LyricFileDTO(file));
            FilesAdded?.Invoke(this, Data);

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
            List<LyricFileDTO> options = new List<LyricFileDTO>();

            foreach (var file in e)
            {
                var dto = new LyricFileDTO(file);
                options.Add(dto);
                Data.Add(dto);
            }
            FilesAdded?.Invoke(this, options);
        }

        private void Service_FilesRemoved(object sender, IEnumerable<LyricFile> e)
        {
            List<LyricFileDTO> options = new List<LyricFileDTO>();

            foreach (var file in e)
            {
                var dto = Data.First(d => d.FilePath == file.Path);
                options.Add(dto);
                Data.Remove(dto);
            }

            FilesRemoved?.Invoke(this, options);
        }
    }
}