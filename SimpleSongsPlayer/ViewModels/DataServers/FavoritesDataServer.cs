using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels
{
    public class FavoritesDataServer
    {
        public static FavoritesDataServer Current = new FavoritesDataServer();

        private UserFavoriteService userFavoriteService;

        private FavoritesDataServer()
        {
            UserFavoritesList.CollectionChanged += UserFavoritesList_CollectionChanged;
        }

        public ObservableCollection<MusicFileGroup> UserFavoritesList { get; } = new ObservableCollection<MusicFileGroup>();

        public async Task InitializeFavoritesService()
        {
            if (userFavoriteService != null)
                return;

            this.LogByObject("获取服务");
            userFavoriteService = UserFavoriteService.GetService(await MusicLibraryService<MusicFile, MusicFileFactory>.GetService());

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
    }
}