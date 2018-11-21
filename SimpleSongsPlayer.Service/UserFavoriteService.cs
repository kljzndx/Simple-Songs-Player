using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.Internal;
using SimpleSongsPlayer.DAL;
using SimpleSongsPlayer.DAL.Factory;

namespace SimpleSongsPlayer.Service
{
    public class UserFavoriteService : IFileService<IGrouping<string, MusicFile>>
    {
        private static UserFavoriteService current;
        
        private readonly ContextHelper<FilesContext, UserFavorite> helper = new ContextHelper<FilesContext, UserFavorite>();

        public event EventHandler<IEnumerable<IGrouping<string, MusicFile>>> FilesAdded;
        public event EventHandler<IEnumerable<IGrouping<string, MusicFile>>> FilesRemoved;
        public event EventHandler<KeyValuePair<string, string>> GroupRenamed;

        private UserFavoriteService(MusicLibraryService<MusicFile, MusicFileFactory> libraryService)
        {
            libraryService.FilesRemoved += MusicLibraryService_FilesRemoved;
        }

        public List<IGrouping<string, MusicFile>> GetFiles()
        {
            var result = QueryMusicFiles();
            return result;
        }

        public void AddRange(string name, IEnumerable<MusicFile> files)
        {
            var list = files.ToList();

            helper.CustomOption(fa =>
            {
                var result = new List<UserFavorite>();

                list.RemoveAll(mf => fa.Any(f => f.GroupName == name && f.FilePath == mf.Path));
                list.ForEach(mf => result.Add(new UserFavorite(name, mf.Path)));

                fa.AddRange(result);
            });

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

        public void RenameGroup(string oldName, string newName)
        {
            helper.CustomOption(fat =>
            {
                List<UserFavorite> list = fat.Where(fa => fa.GroupName == oldName).ToList();
                list.ForEach(fa => fa.GroupName = newName);
                fat.UpdateRange(list);
            });

            GroupRenamed?.Invoke(this, new KeyValuePair<string, string>(oldName, newName));
        }

        private void RemoveRangeInAllGroup(IEnumerable<MusicFile> files)
        {
            var source = files.ToList();
            var list = source.Select(f => f.Path).ToList();
            List<UserFavorite> optionResult = null;

            helper.CustomOption(table =>
            {
                optionResult = table.Where(favorite => list.Contains(favorite.FilePath)).ToList();
                table.RemoveRange(optionResult);
            });
            
            FilesRemoved?.Invoke(this, QueryMusicFiles(optionResult));
        }
        
        private IGrouping<string, MusicFile> CreateGrouping(string name, IEnumerable<MusicFile> files)
        {
            var group = new Grouping<string, MusicFile>(name);
            foreach (var file in files)
                group.Add(file);
            return group;
        }

        private List<IGrouping<string, MusicFile>> QueryMusicFiles(IEnumerable<UserFavorite> target = null)
        {
            var result = new List<IGrouping<string, MusicFile>>();

            using (var db = new FilesContext())
            {
                var source = (target ?? db.UserFavorites).ToList();
                List<string> allPaths = source.Select(fa => fa.FilePath).Distinct().ToList();
                List<MusicFile> allFiles = db.MusicFiles.Where(mf => allPaths.Contains(mf.Path)).ToList();

                foreach (var path in allPaths)
                    if (allFiles.TrueForAll(af => af.Path != path))
                        db.UserFavorites.RemoveRange(db.UserFavorites.Where(fa => fa.FilePath == path));

                foreach (var favorite in source.GroupBy(fa => fa.GroupName))
                {
                    List<string> paths = favorite.Select(fa => fa.FilePath).ToList();
                    result.Add(CreateGrouping(favorite.Key, allFiles.Where(f => paths.Contains(f.Path))));
                }
            }

            return result;
        }

        private void MusicLibraryService_FilesRemoved(object sender, IEnumerable<MusicFile> e)
        {
            RemoveRangeInAllGroup(e);
        }

        public static UserFavoriteService GetService(MusicLibraryService<MusicFile, MusicFileFactory> libraryService)
        {
            if (current is null)
                current = new UserFavoriteService(libraryService);

            return current;
        }
    }
}