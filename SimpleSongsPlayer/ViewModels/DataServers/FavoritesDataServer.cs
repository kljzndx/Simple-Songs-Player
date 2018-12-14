using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.DataServers;

namespace SimpleSongsPlayer.ViewModels
{
    public class FavoritesDataServer
    {
        public static FavoritesDataServer Current = new FavoritesDataServer();

        private ObservableCollection<MusicFileDTO> musicList = MusicLibraryDataServer.Current.MusicFilesList;
        private UserFavoriteService userFavoriteService;
        
        private FavoritesDataServer()
        {
        }

        public IGroupServiceBasicOptions<string> FavoriteOption { get; private set; }
        
        public ObservableCollection<MusicFileGroup> UserFavoritesList { get; } = new ObservableCollection<MusicFileGroup>();

        public async Task InitializeFavoritesService()
        {
            if (userFavoriteService != null)
                return;

            this.LogByObject("获取服务");
            userFavoriteService = UserFavoriteService.GetService(await MusicLibraryService<MusicFile, MusicFileFactory>.GetService());
            FavoriteOption = userFavoriteService;

            this.LogByObject("获取用户收藏");
            foreach (var grouping in await userFavoriteService.GetFiles())
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var path in grouping)
                    files.Add(GetFile(path));

                UserFavoritesList.Add(new MusicFileGroup(grouping.Key, files, f => f.GetAlbumCover()));
            }

            this.LogByObject("监听服务");
            userFavoriteService.FilesAdded += UserFavoriteService_FilesAdded;
            userFavoriteService.FilesRemoved += UserFavoriteService_FilesRemoved;
            userFavoriteService.GroupRenamed += UserFavoriteService_GroupRenamed;
        }

        public async Task MigrateOldFavorites()
        {
            if (userFavoriteService is null)
                await InitializeFavoritesService();

            try
            {
                var options = new QueryOptions(CommonFileQuery.OrderByName, new[] {".plb"});
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("plbs");
                var query = folder.CreateFileQueryWithOptions(options);
                var files = await query.GetFilesAsync();
                foreach (var file in files)
                {
                    var lines = await FileIO.ReadLinesAsync(file);
                    await userFavoriteService.AddRange(file.DisplayName, lines);
                }

                await folder.DeleteAsync();
            }
            catch (Exception)
            {
            }
        }

        private MusicFileDTO GetFile(string path)
        {
            var result = musicList.FirstOrDefault(f => f.FilePath == path);
            if (result != null)
                return result;

            foreach (var fileGroup in UserFavoritesList)
            {
                result = fileGroup.Items.FirstOrDefault(f => f.FilePath == path);
                if (result != null)
                    break;
            }

            return result;
        }
        
        private void UserFavoriteService_FilesAdded(object sender, IEnumerable<IGrouping<string, string>> e)
        {
            this.LogByObject("检测到有收藏的音乐添加，正在同步添加操作");
            foreach (var group in e)
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var path in group)
                    files.Add(GetFile(path));

                var fileGroup = UserFavoritesList.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup != null)
                    foreach (var dto in files.Where(f => fileGroup.Items.All(i => i.FilePath != f.FilePath)))
                        fileGroup.Items.Add(dto);
                else
                    UserFavoritesList.Add(new MusicFileGroup(group.Key, files, f => f.GetAlbumCover()));
            }
        }

        private void UserFavoriteService_FilesRemoved(object sender, IEnumerable<IGrouping<string, string>> e)
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
                    foreach (var path in group)
                        files.Add(GetFile(path));

                    files.ForEach(mf => fileGroup.Items.Remove(mf));
                }
            }
        }

        private void UserFavoriteService_GroupRenamed(object sender, KeyValuePair<string, string> e)
        {
            var group = UserFavoritesList.FirstOrDefault(g => g.Name == e.Key);
            if (group != null)
                group.Name = e.Value;
        }
    }
}