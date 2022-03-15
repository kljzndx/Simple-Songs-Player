using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.EntityFrameworkCore;

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
        private MainDbContext DbContext => Ioc.Default.GetRequiredService<MainDbContext>();

        public MusicFileManageService()
        {
        }

        public List<MusicUi> GetAllMusic()
        {
            return DbContext.MusicFiles.Select(mf => new MusicUi(mf)).ToList();
        }

        public List<MusicUi> GetPlaybackList()
        {
            return DbContext.PlaybackList.Include(pi => pi.File)
                   .OrderBy(pi => pi.TrackId).Select(pi => new MusicUi(pi.File)).ToList();
        }

        public List<string> GetLibraryListInDb()
        {
            return DbContext.MusicFiles.Select(mf => mf.LibraryFolder).Distinct().ToList();
        }

        public List<MusicUi> QueryMusicByLibraryPath(string libraryPath)
        {
            return DbContext.MusicFiles.Where(mf => mf.LibraryFolder == libraryPath)
                   .Select(mf => new MusicUi(mf)).ToList();
        }

        public async Task RemoveMusicData(IEnumerable<MusicUi> source)
        {
            if (!source.Any()) return;

            DbContext.MusicFiles.RemoveRange(source.Select(mu => mu.GetTable()));
            await DbContext.SaveChangesAsync();
            WeakReferenceMessenger.Default.Send("DataRemoved", nameof(MusicFileManageService));
        }

        public List<MusicGroup> GroupMusic(IEnumerable<MusicUi> source, Func<MusicUi, string> GroupKeySelector)
        {
            return source.GroupBy(GroupKeySelector).Select(g => new MusicGroup(g.Key, g)).OrderBy(mg => mg.Name).ToList();
        }

        public List<MusicGroup> GroupMusicByFirstLetter(IEnumerable<MusicUi> source)
        {
            CharacterGroupings cgs = new CharacterGroupings();

            var musicGroupList = source.GroupBy(mu => cgs.Lookup(mu.Title).Replace("拼音", "")).Select(g => new MusicGroup(g.Key, g)).ToList();
            var cgsGroupList = cgs.Where(c => !c.Label.Contains("拼音") && musicGroupList.All(mg => mg.Name != c.Label))
                               .Select(c => new MusicGroup(c.Label)).ToList();

            musicGroupList.AddRange(cgsGroupList);
            return musicGroupList.Where(mg => mg.Items.Any() || !string.IsNullOrWhiteSpace(mg.Name)).OrderBy(mg => mg.Name).ToList();
        }

        public List<MusicGroup> OrderMusicDataBy<TOrderByKey>(IEnumerable<MusicGroup> source, Func<MusicUi, TOrderByKey> orderByKeySelector)
        {
            return source.Select(mg => new MusicGroup(mg.Name, mg.Items.OrderBy(orderByKeySelector).ToList())).ToList();
        }

        public List<MusicGroup> ReverseMusicData(IEnumerable<MusicGroup> source)
        {
            return source.Select(mg => new MusicGroup(mg.Name, mg.Items.Reverse().ToList())).ToList();
        }
    }
}
