using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public class MusicFileDataServer : IFileDataServer<MusicFileDTO>
    {
        public static readonly MusicFileDataServer Current = new MusicFileDataServer();
        private MusicLibraryService<MusicFile, MusicFileFactory> musicFilesService;

        private MusicFileDataServer()
        {
        }

        public bool IsInit { get; private set; }
        public ObservableCollection<MusicFileDTO> Data { get; } = new ObservableCollection<MusicFileDTO>();

        public event EventHandler<IEnumerable<MusicFileDTO>> DataAdded;
        public event EventHandler<IEnumerable<MusicFileDTO>> DataRemoved;
        public event EventHandler<IEnumerable<MusicFileDTO>> DataUpdated;

        public async Task InitializeMusicService()
        {
            if (IsInit)
                return;

            this.LogByObject("获取服务");
            IsInit = true;
            musicFilesService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();

            this.LogByObject("提取数据库里的音乐文件");
            var musicData = await musicFilesService.GetFiles();
            if (musicData.Any())
            {
                foreach (var musicFile in musicData)
                    Data.Add(new MusicFileDTO(musicFile));

                this.LogByObject("触发数据添加事件");
                DataAdded?.Invoke(this, Data);
            }
            
            this.LogByObject("监听服务");
            musicFilesService.FilesAdded += MusicFilesService_FilesAdded;
            musicFilesService.FilesRemoved += MusicFilesService_FilesRemoved;
            musicFilesService.FilesUpdated += MusicFilesService_FilesUpdated;
        }

        public async Task ScanMusicFiles()
        {
            if (!IsInit)
                await InitializeMusicService();
            
            await musicFilesService.ScanFiles();
        }

        private void MusicFilesService_FilesAdded(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有新的文件，正在同步");
            List<MusicFileDTO> option = new List<MusicFileDTO>();

            foreach (var musicFile in e)
            {
                if (Data.All(f => f.FilePath != musicFile.Path))
                {
                    var result = new MusicFileDTO(musicFile);
                    option.Add(result);
                    Data.Add(result);
                }
            }

            DataAdded?.Invoke(this, option);
        }

        private void MusicFilesService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有文件被移除，正在同步");
            List<MusicFileDTO> option = new List<MusicFileDTO>();

            foreach (var musicFile in e)
            {
                if (Data.FirstOrDefault(f => f.FilePath == musicFile.Path) is MusicFileDTO dto)
                {
                    option.Add(dto);
                    Data.Remove(dto);
                }
            }

            DataRemoved?.Invoke(this, option);
        }

        private void MusicFilesService_FilesUpdated(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有文件被更新，正在同步");
            List<MusicFileDTO> option = new List<MusicFileDTO>();

            foreach (var item in e)
            {
                if (Data.FirstOrDefault(f => f.FilePath == item.Path) is MusicFileDTO dto)
                {
                    option.Add(dto);
                    dto.Update(item);
                }
            }

            DataUpdated?.Invoke(this, option);
        }
    }
}