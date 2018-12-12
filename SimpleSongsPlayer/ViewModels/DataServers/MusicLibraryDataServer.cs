using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels.DataServers
{
    public class MusicLibraryDataServer
    {
        public static readonly MusicLibraryDataServer Current = new MusicLibraryDataServer();

        private MusicLibraryService<MusicFile, MusicFileFactory> musicFilesService;

        private MusicLibraryDataServer()
        {
            CoreWindow.GetForCurrentThread().Activated += CoreWindow_Activated;
        }

        public ObservableCollection<MusicFileDTO> MusicFilesList { get; } = new ObservableCollection<MusicFileDTO>();

        public event EventHandler<EventArgs> MusicFilesAdded;

        public async Task InitializeMusicService()
        {
            if (musicFilesService != null)
                return;

            this.LogByObject("获取服务");
            musicFilesService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();

            this.LogByObject("获取音乐文件");
            (await musicFilesService.GetFiles()).ForEach(mf => MusicFilesList.Add(new MusicFileDTO(mf)));

            MusicFilesAdded?.Invoke(this, EventArgs.Empty);

            this.LogByObject("监听服务");
            musicFilesService.FilesAdded += MusicFilesService_FilesAdded;
            musicFilesService.FilesRemoved += MusicFilesService_FilesRemoved;
            musicFilesService.FilesUpdated += MusicFilesService_FilesUpdated;
        }

        public async Task ScanMusicFiles()
        {
            if (musicFilesService is null)
                await InitializeMusicService();

            this.LogByObject("开始扫描音乐文件");
            await musicFilesService.ScanFiles();
        }

        private void MusicFilesService_FilesAdded(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有音乐文件添加，正在同步添加操作");
            foreach (var musicFile in e)
                if (MusicFilesList.All(f => f.FilePath != musicFile.Path))
                    MusicFilesList.Add(new MusicFileDTO(musicFile));

            MusicFilesAdded?.Invoke(this, EventArgs.Empty);
        }

        private void MusicFilesService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有音乐文件被移除，正在同步移除操作");
            foreach (var musicFile in e)
                if (MusicFilesList.Any(f => f.FilePath == musicFile.Path))
                    MusicFilesList.Remove(MusicFilesList.First(mf => mf.FilePath == musicFile.Path));
        }

        private void MusicFilesService_FilesUpdated(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有音乐文件被更新，正在同步更新操作");
            foreach (var item in e)
                if (MusicFilesList.FirstOrDefault(f => f.FilePath == item.Path) is MusicFileDTO dto)
                    dto.Update(item);
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != CoreWindowActivationState.Deactivated && musicFilesService != null)
                musicFilesService.ScanFiles();
        }
    }
}