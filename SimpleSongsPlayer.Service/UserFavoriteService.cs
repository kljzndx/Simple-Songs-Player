using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class UserFavoriteService
    {
        private static UserFavoriteService current;

        private readonly ContextHelper<FilesContext, UserFavorite> helper = new ContextHelper<FilesContext, UserFavorite>();
        private readonly MusicFileFactory factory = new MusicFileFactory();

        public event EventHandler<IEnumerable<IGrouping<string, MusicFile>>> FilesAdded;
        public event EventHandler<IEnumerable<IGrouping<string, MusicFile>>> FilesRemoved;

        private UserFavoriteService()
        {
        }

        public async Task<List<IGrouping<string, MusicFile>>> GetFiles()
        {
            var data = helper.ToList().GroupBy(f => f.GroupName);
            var result = new List<IGrouping<string, MusicFile>>();

            foreach (var favorite in data)
                result.Add(CreateGrouping(favorite.Key, await ConvertToFile(favorite)));

            return result;
        }

        public void AddRange(string name, IEnumerable<MusicFile> files)
        {
            var list = files.ToList();
            var result = new List<UserFavorite>();

            list.ForEach(file => result.Add(new UserFavorite(name, file.Path)));
            helper.AddRange(result);
            FilesAdded?.Invoke(this, new[] {CreateGrouping(name, list)});
        }

        public void RemoveRange(string name, IEnumerable<MusicFile> files)
        {
            var source = files.ToList();
            var list = source.Select(f => f.Path).ToList();
            List<UserFavorite> optionResult = null;

            helper.CustomOption(table =>
            {
                optionResult = table.Where(favorite => favorite.GroupName == name && list.Contains(favorite.FilePath)).ToList();
                table.RemoveRange(optionResult);
            });

            FilesRemoved?.Invoke(this, new[] {CreateGrouping(name, source)});
        }

        public async Task RemoveRangeInAllGroup(IEnumerable<MusicFile> files)
        {
            var source = files.ToList();
            var list = source.Select(f => f.Path).ToList();
            List<UserFavorite> optionResult = null;
            List<IGrouping<string, MusicFile>> result = new List<IGrouping<string, MusicFile>>();

            helper.CustomOption(table =>
            {
                optionResult = table.Where(favorite => list.Contains(favorite.FilePath)).ToList();
                table.RemoveRange(optionResult);
            });

            foreach (var item in optionResult.GroupBy(f => f.GroupName))
                result.Add(CreateGrouping(item.Key, await ConvertToFile(item)));

            FilesRemoved?.Invoke(this, result);
        }

        private IGrouping<string, MusicFile> CreateGrouping(string name, IEnumerable<MusicFile> files)
        {
            var group = new Grouping<string, MusicFile>(name);
            foreach (var file in files)
                group.Add(file);
            return group;
        }

        private async Task<List<MusicFile>> ConvertToFile(IEnumerable<UserFavorite> favorites)
        {
            var result = new List<MusicFile>();
            var rubbish = new List<UserFavorite>();
            foreach (var favorite in favorites)
            {
                try
                {
                    result.Add(await factory.FromFilePath(favorite.FilePath));
                }
                catch (Exception)
                {
                    rubbish.Add(favorite);
                }
            }

            if (rubbish.Any())
                helper.RemoveRange(rubbish);

            return result;
        }

        public static UserFavoriteService GetService()
        {
            if (current is null)
                current = new UserFavoriteService();

            return current;
        }
    }
}