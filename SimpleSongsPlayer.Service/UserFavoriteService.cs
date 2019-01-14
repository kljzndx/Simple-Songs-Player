using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class UserFavoriteService : IFileService<IGrouping<string, string>>, IGroupServiceBasicOptions<string>
    {
        private static UserFavoriteService current;
        
        private readonly ContextHelper<FilesContext, UserFavorite> helper = new ContextHelper<FilesContext, UserFavorite>();

        public event EventHandler<IEnumerable<IGrouping<string, string>>> FilesAdded;
        public event EventHandler<IEnumerable<IGrouping<string, string>>> FilesRemoved;
        public event EventHandler<KeyValuePair<string, string>> GroupRenamed;

        private UserFavoriteService(IFileService<MusicFile> libraryService)
        {
            this.LogByObject("订阅音乐库的文件移除事件");
            libraryService.FilesRemoved += MusicLibraryService_FilesRemoved;
        }

        public async Task<List<IGrouping<string, string>>> GetFiles()
        {
            this.LogByObject("开始对数据进行分组操作");
            var result = CreateGroup(await helper.ToList());
            this.LogByObject("分组完成，返回数据");
            return result.ToList();
        }

        public async Task AddRange(string name, IEnumerable<string> keys)
        {
            this.LogByObject("接收数据");
            var sourcePaths = keys.ToList();
            List<UserFavorite> favorites = new List<UserFavorite>();

            using (var db = new FilesContext())
            {
                this.LogByObject("筛选出有效的路径");
                var oldPaths = db.UserFavorites.Where(uf => uf.GroupName == name).Select(uf => uf.FilePath).ToList();
                var newPaths = db.MusicFiles.Where(mf => sourcePaths.Contains(mf.Path)).Select(mf => mf.Path).ToList();
                var optionPaths = newPaths.Where(np => !oldPaths.Contains(np)).ToList();

                if (optionPaths.Any())
                {
                    this.LogByObject("正在添加数据");
                    foreach (var path in optionPaths)
                        favorites.Add(new UserFavorite(name, path));

                    await db.UserFavorites.AddRangeAsync(favorites);
                    await db.SaveChangesAsync();
                }
            }

            if (favorites.Any())
            {
                this.LogByObject("触发数据添加事件");
                FilesAdded?.Invoke(this, CreateGroup(favorites));
            }
        }

        public async Task RemoveGroup(string name)
        {
            List<UserFavorite> optionTarget = null;

            await helper.CustomOption(table =>
            {
                optionTarget = table.Where(g => g.GroupName == name).ToList();
                if (optionTarget.Any())
                {
                    this.LogByObject("正在移除收藏组");
                    table.RemoveRange(optionTarget);
                }
            });

            if (optionTarget.Any())
            {
                this.LogByObject("触发数据移除事件");
                FilesRemoved?.Invoke(this, CreateGroup(optionTarget));
            }
        }

        public async Task RemoveRange(string name, IEnumerable<string> keys)
        {
            this.LogByObject("接收数据");
            var list = keys.ToList();
            List<UserFavorite> optionTarget = null;

            await helper.CustomOption(table =>
            {
                this.LogByObject("提取出对应项目");
                optionTarget = table.Where(g => g.GroupName == name && list.Contains(g.FilePath)).ToList();
                if (optionTarget.Any())
                {
                    this.LogByObject("正在移除收藏项");
                    table.RemoveRange(optionTarget);
                }
            });

            if (optionTarget.Any())
            {
                this.LogByObject("触发数据移除事件");
                FilesRemoved?.Invoke(this, CreateGroup(optionTarget));
            }
        }

        public async Task RemoveRangeInAllGroup(IEnumerable<string> keys)
        {
            this.LogByObject("接收数据");
            var list = keys.ToList();
            List<UserFavorite> optionTarget = null;

            await helper.CustomOption(table =>
            {
                this.LogByObject("提取出对应项目");
                optionTarget = table.Where(g => list.Contains(g.FilePath)).ToList();
                if (optionTarget.Any())
                {
                    this.LogByObject("正在移除收藏组");
                    table.RemoveRange(optionTarget);
                }
            });

            if (optionTarget.Any())
            {
                this.LogByObject("触发数据移除事件");
                FilesRemoved?.Invoke(this, CreateGroup(optionTarget));
            }
        }

        public async Task RenameGroup(string oldName, string newName)
        {
            bool isFound = false;

            await helper.CustomOption(table =>
            {
                var fas = table.Where(fa => fa.GroupName == oldName).ToList();
                if (fas.Any())
                {
                    this.LogByObject("正在重命名收藏组");
                    isFound = true;
                    foreach (var favorite in fas)
                        favorite.GroupName = newName;
                    table.UpdateRange(fas);
                }
            });
            if (isFound)
            {
                this.LogByObject("触发重命名事件");
                GroupRenamed?.Invoke(this, new KeyValuePair<string, string>(oldName, newName));
            }
        }

        private IEnumerable<IGrouping<string, string>> CreateGroup(IEnumerable<UserFavorite> favorites)
        {
            return favorites.GroupBy(g => g.GroupName, g => g.FilePath);
        }

        private async void MusicLibraryService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("已检测到音乐库发生文件移除操作，正在同步移除");
            await RemoveRangeInAllGroup(e.Select(f => f.Path));
        }

        public static UserFavoriteService GetService(IFileService<MusicFile> libraryService)
        {
            if (current is null)
            {
                typeof(UserFavoriteService).LogByType("创建收藏服务");
                current = new UserFavoriteService(libraryService);
            }

            return current;
        }
    }
}