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

        public void AddItem(MediaPlaybackList target, Song song)
        {
            if (!target.Items.Contains(song.PlaybackItem))
                target.Items.Add(song.PlaybackItem);
        }

        public void Push(Song song)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
            {
                if (!mpl.Items.Contains(song.PlaybackItem))
                {
                    mpl.Items.Insert(0, song.PlaybackItem);
                    mpl.MoveTo(0);
                }
                else
                    mpl.MoveTo((uint) mpl.Items.IndexOf(song.PlaybackItem));
            }
            else
            {
                MediaPlaybackList newMpl = new MediaPlaybackList();
                AddItem(newMpl, song);
                App.Player.Source = newMpl;
            }
        }

        public void Push(IEnumerable<Song> songs)
        {
            MediaPlaybackList mpl = new MediaPlaybackList();
            foreach (var song in songs)
                AddItem(mpl, song);
            App.Player.Source = mpl;
        }

        public void Append(Song song)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
                AddItem(mpl, song);
            else
            {
                MediaPlaybackList newMpl = new MediaPlaybackList();
                AddItem(newMpl, song);
                App.Player.Source = newMpl;
            }
        }

        public void Append(IEnumerable<Song> songs)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
                foreach (var song in songs)
                    AddItem(mpl, song);
            else
            {
                MediaPlaybackList newMpl = new MediaPlaybackList();
                foreach (var song in songs)
                    AddItem(newMpl, song);
                App.Player.Source = newMpl;
            }
        }
    }
}