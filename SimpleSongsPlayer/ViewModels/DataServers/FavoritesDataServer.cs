using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Search;
using Newtonsoft.Json;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.DataServers;

namespace SimpleSongsPlayer.ViewModels
{
    public class FavoritesDataServer : IDataServer<MusicFileGroup, KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>>
    {
        public static FavoritesDataServer Current = new FavoritesDataServer();

        private ObservableCollection<MusicFileDTO> musicList = MusicFileDataServer.Current.Data;
        private UserFavoriteService userFavoriteService;
        
        private FavoritesDataServer()
        {
        }

        public bool IsInit { get; private set; }
        public IGroupServiceBasicOptions<string> FavoriteOption => userFavoriteService;
        
        public ObservableCollection<MusicFileGroup> Data { get; } = new ObservableCollection<MusicFileGroup>();

        public event EventHandler<IEnumerable<KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>>> DataAdded;
        public event EventHandler<IEnumerable<KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>>> DataRemoved;
        public event EventHandler<KeyValuePair<string, string>> GroupRenamed;

        public async Task InitializeFavoritesService()
        {
            if (IsInit)
                return;

            this.LogByObject("获取服务");
            IsInit = true;
            userFavoriteService = UserFavoriteService.GetService(await MusicLibraryFileServiceManager.Current.GetMusicFileService());

            this.LogByObject("获取用户收藏");
            var result = new List<KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>>();
            foreach (var grouping in await userFavoriteService.GetData())
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var path in grouping)
                    files.Add(GetFile(path));

                var dto = new MusicFileGroup(grouping.Key, files, f => f.GetAlbumCover());
                Data.Add(dto);
                result.Add(new KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>(dto, files));
            }

            DataAdded?.Invoke(this, result);

            this.LogByObject("监听服务");
            userFavoriteService.DataAdded += UserFavoriteService_DataAdded;
            userFavoriteService.DataRemoved += UserFavoriteService_DataRemoved;
            userFavoriteService.GroupRenamed += UserFavoriteService_GroupRenamed;
        }

        public async Task MigrateOldFavorites()
        {
            if (IsInit)
                await InitializeFavoritesService();

            try
            {
                this.LogByObject("获取播放列表文件夹");
                var options = new QueryOptions(CommonFileQuery.OrderByName, new[] {".plb"});
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("plbs");
                var query = folder.CreateFileQueryWithOptions(options);

                this.LogByObject("提取全部播放列表文件");
                var files = await query.GetFilesAsync();

                if (files.Any())
                {
                    this.LogByObject("开始迁移");
                    foreach (var file in files)
                    {
                        var lines = await FileIO.ReadLinesAsync(file);
                        await userFavoriteService.AddRange(file.DisplayName, lines);
                    }
                }

                this.LogByObject("删除播放列表文件夹");
                await folder.DeleteAsync();
            }
            catch (Exception ex)
            {
                this.LogByObject(ex, "迁移失败");
            }
        }
        
        public string ToJson()
        {
            var data = Data.Select(d => new {Name = d.Name, Paths = d.Items.Select(f => f.FilePath)});
            return JsonConvert.SerializeObject(data);
        }

        private MusicFileDTO GetFile(string path)
        {
            var result = musicList.FirstOrDefault(f => f.FilePath == path);
            if (result != null)
                return result;

            foreach (var fileGroup in Data)
            {
                result = fileGroup.Items.FirstOrDefault(f => f.FilePath == path);
                if (result != null)
                    break;
            }

            return result;
        }
        
        private void UserFavoriteService_DataAdded(object sender, IEnumerable<IGrouping<string, string>> e)
        {
            this.LogByObject("检测到有收藏的音乐添加，正在同步添加操作");
            var result = new List<KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>>();
            foreach (var group in e)
            {
                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var path in group)
                    files.Add(GetFile(path));

                var fileGroup = Data.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup != null)
                {
                    var g = files.Where(f => fileGroup.Items.All(i => i.FilePath != f.FilePath)).ToList();
                    foreach (var dto in g)
                        fileGroup.Items.Add(dto);

                    result.Add(new KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>(fileGroup, g));
                }
                else
                {
                    var dto = new MusicFileGroup(group.Key, files, f => f.GetAlbumCover());
                    Data.Add(dto);
                    result.Add(new KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>(dto, files));
                }
            }

            DataAdded?.Invoke(this, result);
        }

        private void UserFavoriteService_DataRemoved(object sender, IEnumerable<IGrouping<string, string>> e)
        {
            this.LogByObject("检测到有收藏的音乐被移除，正在同步移除操作");
            var result = new List<KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>>();
            foreach (var group in e)
            {
                MusicFileGroup fileGroup = Data.FirstOrDefault(uf => uf.Name == group.Key);
                if (fileGroup is null)
                    continue;

                List<MusicFileDTO> files = new List<MusicFileDTO>();
                foreach (var path in group)
                    files.Add(GetFile(path));

                if (group.Count() >= fileGroup.Items.Count)
                {
                    fileGroup.Items.Clear();
                    Data.Remove(fileGroup);
                }
                else
                    files.ForEach(mf => fileGroup.Items.Remove(mf));

                result.Add(new KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>(fileGroup, files));
            }

            DataRemoved?.Invoke(this, result);
        }

        private void UserFavoriteService_GroupRenamed(object sender, KeyValuePair<string, string> e)
        {
            var group = Data.FirstOrDefault(g => g.Name == e.Key);
            if (group != null)
            {
                this.LogByObject("应用重命名操作");
                group.Name = e.Value;

                GroupRenamed?.Invoke(this, e);
            }
        }
    }
}