using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.Views;

namespace SimpleSongsPlayer.ViewModels
{
    public class DataServer
    {
        public static readonly DataServer Current = GetServer();

        private MusicLibraryService<MusicFile, MusicFileFactory> musicFilesService;
        private UserFavoriteService userFavoriteService;

        public ObservableCollection<MusicFileDTO> MusicFilesList { get; } = new ObservableCollection<MusicFileDTO>();
        public ObservableCollection<MusicFileGroup> UserFavoritesList { get; } = new ObservableCollection<MusicFileGroup>();

        private DataServer()
        {
            UserFavoritesList.CollectionChanged += UserFavoritesList_CollectionChanged;
            CoreWindow.GetForCurrentThread().Activated += CoreWindow_Activated;
        }

        public async Task InitializeMusicService()
        {
            if (musicFilesService != null)
                return;

            this.LogByObject("获取服务");
            musicFilesService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();

            this.LogByObject("获取音乐文件");
            (await musicFilesService.GetFiles()).ForEach(mf => MusicFilesList.Add(new MusicFileDTO(mf)));

            this.LogByObject("监听服务");
            musicFilesService.FilesAdded += MusicFilesService_FilesAdded;
            musicFilesService.FilesRemoved += MusicFilesService_FilesRemoved;
        }

        public async Task InitializeFavoritesService()
        {
            if (userFavoriteService != null)
                return;

            this.LogByObject("获取服务");
            userFavoriteService = UserFavoriteService.GetService(musicFilesService);

            this.LogByObject("获取用户收藏");
            foreach (var grouping in await userFavoriteService.GetFiles())
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var file in grouping)
                    files.Add(new MusicFileDTO(file));

                UserFavoritesList.Add(new MusicFileGroup(grouping.Key, files, f => f.GetAlbumCover()));
            }

            this.LogByObject("监听服务");
            userFavoriteService.FilesAdded += UserFavoriteService_FilesAdded;
            userFavoriteService.FilesRemoved += UserFavoriteService_FilesRemoved;
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
        }

        private void MusicFilesService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有音乐文件被移除，正在同步移除操作");
            foreach (var musicFile in e)
                if (MusicFilesList.Any(f => f.FilePath == musicFile.Path))
                    MusicFilesList.Remove(MusicFilesList.First(mf => mf.FilePath == musicFile.Path));
        }

        private void UserFavoriteService_FilesAdded(object sender, IEnumerable<IGrouping<string, MusicFile>> e)
        {
            this.LogByObject("检测到有收藏的音乐添加，正在同步添加操作");
            foreach (var group in e)
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var file in group)
                    files.Add(new MusicFileDTO(file));

                var fileGroup = UserFavoritesList.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup != null)
                    foreach (var dto in files.Where(f => fileGroup.Items.All(i => i.FilePath != f.FilePath)))
                        fileGroup.Items.Add(dto);
                else
                    UserFavoritesList.Add(new MusicFileGroup(group.Key, files, f => f.GetAlbumCover()));
            }
        }

        private void UserFavoriteService_FilesRemoved(object sender, IEnumerable<IGrouping<string, MusicFile>> e)
        {
            this.LogByObject("检测到有收藏的音乐被移除，正在同步移除操作");
            foreach (var group in e)
            {
                MusicFileGroup fileGroup = UserFavoritesList.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup is null)
                    continue;
                
                if (fileGroup.Items.Count >= group.Count())
                    UserFavoritesList.Remove(fileGroup);
                else
                {
                    List<MusicFileDTO> files = new List<MusicFileDTO>();
                    foreach (var file in group)
                        files.Add(new MusicFileDTO(file));

                    files.ForEach(mf => fileGroup.Items.Remove(mf));
                }
            }
        }

        private async void UserFavoritesList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (userFavoriteService is null)
                return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (MusicFileGroup item in e.NewItems)
                    {
                        await userFavoriteService.AddRange(item.Name, item.Items.Select(f => f.FilePath));
                        item.SetUpService(userFavoriteService);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (MusicFileGroup item in e.OldItems)
                        await userFavoriteService.RemoveGroup(item.Name);
                    break;
            }
        }

        private void CoreWindow_Activated(CoreWindow sender, WindowActivatedEventArgs args)
        {
            if (args.WindowActivationState != CoreWindowActivationState.Deactivated && musicFilesService != null)
                musicFilesService.ScanFiles();
        }

        private static DataServer GetServer()
        {
            typeof(DataServer).LogByType("获取数据服务器");
            return new DataServer();
        }
    }
}