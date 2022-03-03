using SimpleSongsPlayer.Dal;
using SimpleSongsPlayer.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SimpleSongsPlayer.Services
{
    public class MusicFileManageService
    {
        private MainDbContext _dbContext;

        public MusicFileManageService(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        private IQueryable<MusicUi> QueryAllMusic()
        {
            return _dbContext.MusicFiles.Select(mf => new MusicUi(mf));
        }

        public List<MusicGroup> WatchMusicGroup(MusicGroup group)
        {
            return new List<MusicGroup>(new[] { new MusicGroup("All", group.Items) });
        }

        public List<MusicGroup> GetAllMusic()
        {
            return new List<MusicGroup>(new[] { new MusicGroup("All", QueryAllMusic().ToList()) });
        }

        public List<MusicGroup> GroupMusic(Expression<Func<MusicUi, string>> GroupKeySelector)
        {
            return QueryAllMusic().GroupBy(GroupKeySelector).Select(gp => new MusicGroup(gp.Key, gp)).ToList();
        }

        public List<MusicAlbum> GroupMusicAlbum()
        {
            return QueryAllMusic().GroupBy(mu => mu.Album).Select(gp => new MusicAlbum(gp.Key, gp)).ToList();
        }

        public List<MusicGroup> GetPlaybackList()
        {
            var list = _dbContext.MusicFiles.Where(mf => mf.IsInPlaybackList).Select(mf => new MusicUi(mf)).ToList();

            return new List<MusicGroup>(new[] { new MusicGroup("All", list) });
        }

        public async Task SavePlaybackList(IEnumerable<MusicUi> source)
        {
            foreach (var item in source)
                item.IsInPlaybackList = true;

            var mfList = source.Select(mf => mf.GetTable()).ToList();

            _dbContext.MusicFiles.UpdateRange(mfList);
            await _dbContext.SaveChangesAsync();
        }

        public void MusicDataOrderBy<TOrderByKey>(IEnumerable<MusicGroup> source, Func<MusicUi, TOrderByKey> orderByKeySelector, bool isDescending)
        {
            foreach (var group in source)
                if (isDescending)
                    group.Items = group.Items.OrderByDescending(orderByKeySelector).ToList();
                else
                    group.Items = group.Items.OrderBy(orderByKeySelector).ToList();
        }
    }
}
