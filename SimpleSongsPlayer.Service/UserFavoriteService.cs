using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class UserFavoriteService : IFileService<IGrouping<string, MusicFile>>, IGroupServiceBasicOptions<string, MusicFile>
    {
        private static UserFavoriteService current;
        
        private readonly ContextHelper<FilesContext, UserFavorite> helper = new ContextHelper<FilesContext, UserFavorite>();

        public event EventHandler<IEnumerable<IGrouping<string, MusicFile>>> FilesAdded;
        public event EventHandler<IEnumerable<IGrouping<string, MusicFile>>> FilesRemoved;
        public event EventHandler<KeyValuePair<string, string>> GroupRenamed;

        private UserFavoriteService(MusicLibraryService<MusicFile, MusicFileFactory> libraryService)
        {
            this.LogByObject("订阅音乐库的文件移除事件");
            libraryService.FilesRemoved += MusicLibraryService_FilesRemoved;
        }

        public List<IGrouping<string, MusicFile>> GetFiles()
        {
            this.LogByObject("开始对数据进行分组操作");
            var result = QueryMusicFiles();
            this.LogByObject("分组完成，返回数据");
            return result;
        }

        public void AddRange(string name, IEnumerable<MusicFile> files)
        {
            this.LogByObject("接收文件");
            var list = files.ToList();

            helper.CustomOption(fa =>
            {
                this.LogByObject("准备结果集合");
                var result = new List<UserFavorite>();

                this.LogByObject("筛选掉已有的收藏");
                list.RemoveAll(mf => fa.Any(f => f.GroupName == name && f.FilePath == mf.Path));
                this.LogByObject("生成收藏项并添加到结果集合");
                list.ForEach(mf => result.Add(new UserFavorite(name, mf.Path)));

                this.LogByObject("应用结果集合");
                fa.AddRange(result);
            });

            this.LogByObject("触发收藏添加事件");
            FilesAdded?.Invoke(this, new[] {CreateGrouping(name, list)});
        }

        public void RemoveGroup(string name)
        {
            this.LogByObject("准备操作结果集合");
            List<UserFavorite> optionResult = new List<UserFavorite>();

            this.LogByObject("开始从数据库删数据");
            helper.CustomOption(table =>
            {
                optionResult.AddRange(table.Where(favorite => favorite.GroupName == name));
                table.RemoveRange(optionResult);
            });

            this.LogByObject("触发收藏删除事件");
            FilesRemoved?.Invoke(this, QueryMusicFiles(optionResult));
        }

        public void RemoveRange(string name, IEnumerable<MusicFile> files)
        {
            this.LogByObject("接收文件");
            var source = files.ToList();
            this.LogByObject("提取出文件路径");
            var list = source.Select(f => f.Path).ToList();

            this.LogByObject("开始从数据库删数据");
            helper.CustomOption(table => table.RemoveRange(table.Where(favorite => favorite.GroupName == name && list.Contains(favorite.FilePath))));

            this.LogByObject("触发收藏删除事件");
            FilesRemoved?.Invoke(this, new[] {CreateGrouping(name, source)});
        }

        public void RenameGroup(string oldName, string newName)
        {
            helper.CustomOption(fat =>
            {
                this.LogByObject("筛选出要改名的收藏");
                List<UserFavorite> list = fat.Where(fa => fa.GroupName == oldName).ToList();
                this.LogByObject("开始改名");
                list.ForEach(fa => fa.GroupName = newName);
                this.LogByObject("应用改名操作");
                fat.UpdateRange(list);
            });

            this.LogByObject("触发重命名事件");
            GroupRenamed?.Invoke(this, new KeyValuePair<string, string>(oldName, newName));
        }

        private void RemoveRangeInAllGroup(IEnumerable<MusicFile> files)
        {
            this.LogByObject("接收文件");
            var source = files.ToList();
            this.LogByObject("提取出文件路径");
            var list = source.Select(f => f.Path).ToList();

            this.LogByObject("准备操作集合");
            List<UserFavorite> optionResult = null;
            
            this.LogByObject("连接数据库");
            helper.CustomOption(table =>
            {
                this.LogByObject("将要移除的数据添加到集合中");
                optionResult = table.Where(favorite => list.Contains(favorite.FilePath)).ToList();
                this.LogByObject("应用操作集合");
                table.RemoveRange(optionResult);
            });
            
            this.LogByObject("触发收藏删除事件");
            FilesRemoved?.Invoke(this, QueryMusicFiles(optionResult));
        }
        
        private IGrouping<string, MusicFile> CreateGrouping(string name, IEnumerable<MusicFile> files)
        {
            this.LogByObject("准备空的组");
            var group = new Grouping<string, MusicFile>(name);

            this.LogByObject("将文件添加到组中");
            foreach (var file in files)
                group.Add(file);

            this.LogByObject("返回组数据");
            return group;
        }

        private List<IGrouping<string, MusicFile>> QueryMusicFiles(IEnumerable<UserFavorite> target = null)
        {
            this.LogByObject("准备操作结果集合");
            var result = new List<IGrouping<string, MusicFile>>();

            using (var db = new FilesContext())
            {
                this.LogByObject("获取数据源");
                var source = (target ?? db.UserFavorites).ToList();
                this.LogByObject("从数据源提取出路径");
                List<string> allPaths = source.Select(fa => fa.FilePath).Distinct().ToList();
                this.LogByObject("从数据库根据路径查询文件");
                List<MusicFile> allFiles = db.MusicFiles.Where(mf => allPaths.Contains(mf.Path)).ToList();

                this.LogByObject("清理垃圾收藏");
                foreach (var path in allPaths)
                    if (allFiles.TrueForAll(af => af.Path != path))
                        db.UserFavorites.RemoveRange(db.UserFavorites.Where(fa => fa.FilePath == path));

                this.LogByObject("开始对文件分组并添加至结果集合");
                foreach (var favorite in source.GroupBy(fa => fa.GroupName))
                {
                    List<string> paths = favorite.Select(fa => fa.FilePath).ToList();
                    result.Add(CreateGrouping(favorite.Key, allFiles.Where(f => paths.Contains(f.Path))));
                }
            }

            this.LogByObject("返回分组数据");
            return result;
        }

        private void MusicLibraryService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            this.LogByObject("已检测到音乐库发生文件移除操作，正在同步移除");
            RemoveRangeInAllGroup(e);
        }

        public static UserFavoriteService GetService(MusicLibraryService<MusicFile, MusicFileFactory> libraryService)
        {
            typeof(UserFavoriteService).LogByType("获取用户收藏服务");
            if (current is null)
                current = new UserFavoriteService(libraryService);

            return current;
        }
    }
}