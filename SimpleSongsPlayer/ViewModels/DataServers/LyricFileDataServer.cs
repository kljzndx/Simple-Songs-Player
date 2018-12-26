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
    public class LyricFileDataServer : IFileDataServer<LyricFileDTO>
    {
        public static readonly LyricFileDataServer Current = new LyricFileDataServer();

        private MusicLibraryService<LyricFile, LyricFileFactory> service;

        private LyricFileDataServer()
        {
        }

        public bool IsInit { get; private set; }
        public ObservableCollection<LyricFileDTO> Data { get; } = new ObservableCollection<LyricFileDTO>();
        public event EventHandler<IEnumerable<LyricFileDTO>> DataAdded;
        public event EventHandler<IEnumerable<LyricFileDTO>> DataRemoved;

        public async Task Init()
        {
            if (IsInit)
                return;

            IsInit = true;
            service = await MusicLibraryService<LyricFile, LyricFileFactory>.GetService();
            var files = await service.GetFiles();
            foreach (var file in files)
                Data.Add(new LyricFileDTO(file));
            DataAdded?.Invoke(this, Data);

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
            DataAdded?.Invoke(this, options);
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

            DataRemoved?.Invoke(this, options);
        }
    }
}