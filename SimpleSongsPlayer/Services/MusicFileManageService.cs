using SimpleSongsPlayer.Dal;
using SimpleSongsPlayer.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

using Windows.Globalization.Collation;

namespace SimpleSongsPlayer.Services
{
    public class MusicFileManageService
    {
        private MainDbContext _dbContext;

        public MusicFileManageService(MainDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<MusicUi> GetAllMusic()
        {
            return _dbContext.MusicFiles.Select(mf => new MusicUi(mf)).ToList();
        }

        public List<MusicGroup> GroupMusic(IEnumerable<MusicUi> source, Func<MusicUi, string> GroupKeySelector)
        {
            return source.GroupBy(GroupKeySelector).Select(g => new MusicGroup(g.Key, g)).ToList();
        }

        public List<MusicGroup> GroupMusicByFirstLetter(IEnumerable<MusicUi> source)
        {
            CharacterGroupings cgs = new CharacterGroupings();

            var musicGroupList = GroupMusic(source, mu => cgs.Lookup(mu.Title).Replace("拼音", ""));
            var cgsGroupList = cgs.Where(c => !c.Label.Contains("拼音") && musicGroupList.All(mg => mg.Name != c.Label))
                               .Select(c => new MusicGroup(c.Label)).ToList();

            musicGroupList.AddRange(cgsGroupList);
            return musicGroupList;
        }

        public List<MusicAlbum> GetMusicAlbumList()
        {
            return GetAllMusic().GroupBy(mu => mu.Album).Select(g => new MusicAlbum(g.Key, g)).ToList();
        }

        public List<MusicGroup> WatchMusicGroup(MusicGroup group)
        {
            return new List<MusicGroup>(new[] { new MusicGroup("All", group.Items) });
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
