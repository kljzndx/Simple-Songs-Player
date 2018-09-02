using System.Collections.Generic;
using System.Linq;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public abstract class SongViewModelBase : ViewModelBase
    {
        private readonly SongsGroupsFactoryBase songsGroupsFactory;
        private CollectionViewSource groupsDataSource;

        protected SongViewModelBase(SongsGroupsFactoryBase factory)
        {
            songsGroupsFactory = factory;
        }

        public List<SongsGroup> SongGroups { get; private set; }

        public CollectionViewSource GroupsDataSource
        {
            get => groupsDataSource;
            set => Set(ref groupsDataSource, value);
        }

        public void RefreshData(List<Song> allSongs)
        {
            SongGroups = songsGroupsFactory.ClassifySongsGroups(allSongs);

            GroupsDataSource = new CollectionViewSource
            {
                IsSourceGrouped = true,
                ItemsPath = new PropertyPath("Items"),
                Source = SongGroups
            };
        }

        /// <summary>
        /// 获取给定组中的所有歌曲
        /// </summary>
        /// <param name="groupName">
        /// 要获取所有歌曲的组名   
        /// 
        /// 提示： 为 "$all$" 时将获取所有组中的歌曲
        /// </param>
        /// <returns>该组下的所有歌曲</returns>
        public List<Song> GetSongs(string groupName)
        {
            var group = groupName == "$all$" ? SongGroups : SongGroups.Where(sg => sg.Name == groupName);
            return group.Select(sg => sg.Items).Aggregate((s, n) =>
            {
                List<Song> r = new List<Song>();
                r.AddRange(s);
                r.AddRange(n);
                return r;
            }).ToList();
        }

        public void SetPlayerSource(Song theSong, IEnumerable<Song> source)
        {
            List<Song> songs = source.ToList();
            MediaPlaybackList mpl = new MediaPlaybackList();
            foreach (var song in songs)
                mpl.Items.Add(song.PlaybackItem);

            bool isEqual = false;
            MediaPlaybackList currentMpl = App.Player.Source as MediaPlaybackList;
            if (currentMpl != null && currentMpl.Items.Count == mpl.Items.Count)
            {
                for (int i = 0; i < mpl.Items.Count; i++)
                {
                    isEqual = mpl.Items[i].Equals(currentMpl.Items[i]);
                    if (!isEqual)
                        break;
                }
            }

            if (!isEqual)
            {
                App.Player.Source = mpl;
                App.Player.Play();
                mpl.MoveTo((uint) songs.IndexOf(theSong));
            }
            else
                currentMpl.MoveTo((uint) songs.IndexOf(theSong));
        }
    }
}