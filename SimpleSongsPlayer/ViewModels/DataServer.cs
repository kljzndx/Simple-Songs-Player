using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
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
        private static DataServer current;

        public MusicLibraryService<MusicFile, MusicFileFactory> MusicFilesService;
        public UserFavoriteService UserFavoriteService;

        public readonly ObservableCollection<MusicFileDTO> MusicFilesList = new ObservableCollection<MusicFileDTO>();
        public readonly ObservableCollection<MusicFileGroup> UserFavoritesList = new ObservableCollection<MusicFileGroup>();

        private DataServer()
        {
            UserFavoritesList.CollectionChanged += UserFavoritesList_CollectionChanged;
        }

        private async Task Initialize()
        {
            this.LogByObject("获取服务");
            MusicFilesService = await MusicLibraryService<MusicFile, MusicFileFactory>.GetService();
            UserFavoriteService = UserFavoriteService.GetService(MusicFilesService);

            this.LogByObject("获取音乐文件");
            MusicFilesService.GetFiles().ForEach(mf => MusicFilesList.Add(new MusicFileDTO(mf)));

            this.LogByObject("获取用户收藏");
            foreach (var grouping in UserFavoriteService.GetFiles())
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var file in grouping)
                    files.Add(new MusicFileDTO(file));

                UserFavoritesList.Add(new MusicFileGroup(grouping.Key, files, await files.First().GetAlbumCover()));
            }

            this.LogByObject("监听服务");
            MusicFilesService.FilesAdded += MusicFilesService_FilesAdded;
            MusicFilesService.FilesRemoved += MusicFilesService_FilesRemoved;
            UserFavoriteService.FilesAdded += UserFavoriteService_FilesAdded;
            UserFavoriteService.FilesRemoved += UserFavoriteService_FilesRemoved;
        }

        private void MusicFilesService_FilesAdded(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有音乐文件添加，正在同步添加操作");
            foreach (var musicFile in e)
                MusicFilesList.Add(new MusicFileDTO(musicFile));
        }

        private void MusicFilesService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("检测到有音乐文件被移除，正在同步移除操作");
            foreach (var musicFile in e)
                MusicFilesList.Remove(MusicFilesList.First(mf => mf.FilePath == musicFile.Path));
        }

        private async void UserFavoriteService_FilesAdded(object sender, IEnumerable<IGrouping<string, MusicFile>> e)
        {
            this.LogByObject("检测到有收藏的音乐添加，正在同步添加操作");
            foreach (var group in e)
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var file in group)
                    files.Add(new MusicFileDTO(file));

                var fileGroup = UserFavoritesList.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup != null)
                    files.ForEach(fileGroup.Items.Add);
                else
                    UserFavoritesList.Add(new MusicFileGroup(group.Key, files, await files.First().GetAlbumCover()));
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

        private void UserFavoritesList_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (MusicFileGroup item in e.NewItems)
                    {
                        UserFavoriteService.AddRange(item.Name, item.Items.Select(f => f.FilePath));
                        item.SetUpService(UserFavoriteService);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (MusicFileGroup item in e.OldItems)
                        UserFavoriteService.RemoveGroup(item.Name);
                    break;
            }
        }

        public static async Task<DataServer> GetServer()
        {
            typeof(DataServer).LogByType("获取数据服务器");
            if (current is null)
            {
                current = new DataServer();
                await current.Initialize();
            }

            return current;
        }
    }
}