using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public class LyricFileDataServer : IFileDataServer<LyricFileDTO>
    {
        public static readonly LyricFileDataServer Current = new LyricFileDataServer();

        private MusicLibraryFileService<LyricFile, LyricFileFactory> service;

        private LyricFileDataServer()
        {
        }

        public bool IsInit { get; private set; }
        public ObservableCollection<LyricFileDTO> Data { get; } = new ObservableCollection<LyricFileDTO>();
        public event EventHandler<IEnumerable<LyricFileDTO>> DataAdded;
        public event EventHandler<IEnumerable<LyricFileDTO>> DataRemoved;
        public event EventHandler<IEnumerable<LyricFileDTO>> DataUpdated;

        public async Task Init()
        {
            if (IsInit)
                return;

            this.LogByObject("获取服务");
            IsInit = true;
            service = await MusicLibraryFileServiceManager.Current.GetLyricFileService();

            this.LogByObject("提取数据库里的歌词文件");
            var files = await service.GetData();

            if (files.Any())
            {
                foreach (var file in files)
                    Data.Add(new LyricFileDTO(file));

                this.LogByObject("触发数据添加事件");
                DataAdded?.Invoke(this, Data);
            }

            this.LogByObject("监听服务");
            service.DataAdded += Service_DataAdded;
            service.DataRemoved += Service_DataRemoved;
            service.DataUpdated += Service_DataUpdated;
        }

        private void Service_DataAdded(object sender, IEnumerable<LyricFile> e)
        {
            this.LogByObject("检测到有新的文件，正在同步");
            List<LyricFileDTO> options = new List<LyricFileDTO>();

            foreach (var file in e)
            {
                var dto = new LyricFileDTO(file);
                options.Add(dto);
                Data.Add(dto);
            }

            DataAdded?.Invoke(this, options);
        }

        private void Service_DataRemoved(object sender, IEnumerable<LyricFile> e)
        {
            this.LogByObject("检测到有文件被移除，正在同步");
            List<LyricFileDTO> options = new List<LyricFileDTO>();

            foreach (var file in e)
            {
                var dto = Data.First(d => d.FilePath == file.Path);
                options.Add(dto);
                Data.Remove(dto);
            }

            DataRemoved?.Invoke(this, options);
        }

        private async void Service_DataUpdated(object sender, IEnumerable<LyricFile> e)
        {
            this.LogByObject("检测到有文件被更改，正在同步");
            List<LyricFileDTO> options = new List<LyricFileDTO>();

            foreach (var file in e)
            {
                var dto = Data.FirstOrDefault(d => d.FilePath == file.Path);
                if (dto is null)
                    continue;

                options.Add(dto);
                await dto.Update(file);
            }

            DataUpdated?.Invoke(this, options);
        }
    }
}