using System;
using System.Collections.Generic;
using System.Linq;
using SimpleSongsPlayer.DAL;

namespace SimpleSongsPlayer.Service
{
    public class UserFavoriteService : IFileService<UserFavorite>
    {
        private static UserFavoriteService current;

        private readonly ContextHelper<FilesContext, UserFavorite> helper = new ContextHelper<FilesContext, UserFavorite>();

        public event EventHandler<IEnumerable<UserFavorite>> FilesAdded;
        public event EventHandler<IEnumerable<UserFavorite>> FilesRemoved;

        private UserFavoriteService()
        {
        }

        public List<UserFavorite> GetFiles()
        {
            return helper.ToList();
        }

        public void AddRange(string name, IEnumerable<MusicFile> files)
        {
            var list = files.ToList();
            var result = new List<UserFavorite>();

            list.ForEach(file => result.Add(new UserFavorite(name, file)));
            helper.AddRange(result);
            FilesAdded?.Invoke(this, result);
        }

        public void RemoveRange(IEnumerable<MusicFile> files)
        {
            var list = files.ToList();
            List<UserFavorite> result = null;

            helper.CustomOption(table =>
            {
                result = table.Where(favorite => list.Contains(favorite.File)).ToList();
                table.RemoveRange(result);
            });
            FilesRemoved?.Invoke(this, result);
        }

        public static UserFavoriteService GetService()
        {
            if (current is null)
                current = new UserFavoriteService();

            return current;
        }
    }
}