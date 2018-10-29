using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using GalaSoft.MvvmLight;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.Factories;

namespace SimpleSongsPlayer.ViewModels.SongViewModels
{
    public abstract class SongViewModelBase : ViewModelBase
    {
        private const string NowPlayingList = "正在播放列表";

        private readonly ISongsGroupsFactory songsGroupsFactory;
        private readonly ISongGroupsAsyncFactory songGroupsAsyncFactory;

        private CollectionViewSource groupsDataSource;

        protected SongViewModelBase(ISongsGroupsFactory factory)
        {
            songsGroupsFactory = factory;
        }

        protected SongViewModelBase(ISongGroupsAsyncFactory factory)
        {
            songGroupsAsyncFactory = factory;
        }

        public ObservableCollection<SongsGroup> SongGroups { get; private set; }

        public CollectionViewSource GroupsDataSource
        {
            get => groupsDataSource;
            set => Set(ref groupsDataSource, value);
        }

        public virtual async Task RefreshData(List<Song> allSongs)
        {
            LoggerMembers.VmLogger.Info("正在对给定歌曲进行分组");
            if (songGroupsAsyncFactory != null)
                SongGroups = await songGroupsAsyncFactory.ClassifySongGroupsAsync(allSongs);
            else
                SongGroups = songsGroupsFactory.ClassifySongGroups(allSongs);

            LoggerMembers.VmLogger.Info("完成歌曲分组， 正在创建绑定源");
            GroupsDataSource = new CollectionViewSource
            {
                IsSourceGrouped = true,
                ItemsPath = new PropertyPath("Items"),
                Source = SongGroups
            };
            LoggerMembers.VmLogger.Info("完成绑定源创建");
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
        public List<SongItem> GetSongs(string groupName)
        {
            bool isAll = groupName == "$all$";

            LoggerMembers.VmLogger.Info($"正在获取 {(isAll?"所有":groupName)} 组中的歌曲");
            var group = isAll ? SongGroups : SongGroups.Where(sg => sg.Name == groupName);
            var result = group.Select(sg => sg.Items).Aggregate((s, n) =>
            {
                List<SongItem> r = new List<SongItem>();
                r.AddRange(s);
                r.AddRange(n);
                return new ObservableCollection<SongItem>(r);
            }).ToList();

            LoggerMembers.VmLogger.Info("完成歌曲获取");
            return result;
        }

        private void AddItem(MediaPlaybackList target, SongItem song)
        {
            if (!target.Items.Contains(song.PlaybackItem))
                target.Items.Add(song.PlaybackItem);
        }

        public void Push(SongItem song)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
            {
                if (!mpl.Items.Contains(song.PlaybackItem))
                {
                    LoggerMembers.VmLogger.Info($"正在添加歌曲至{NowPlayingList}顶端");
                    mpl.Items.Insert(0, song.PlaybackItem);
                    LoggerMembers.VmLogger.Info("正在切换到该歌曲");
                    mpl.MoveTo(0);
                    LoggerMembers.VmLogger.Info("完成切换");
                }
                else
                {
                    LoggerMembers.VmLogger.Info($"{NowPlayingList}里已有该歌曲， 正在切换到该歌曲");
                    mpl.MoveTo((uint) mpl.Items.IndexOf(song.PlaybackItem));
                    LoggerMembers.VmLogger.Info("完成切换");
                }

                if (App.Player.PlaybackSession is null || App.Player.PlaybackSession.PlaybackState != MediaPlaybackState.Playing)
                    App.Player.Play();
            }
            else
                Push(new[] {song});
        }

        public void Push(IEnumerable<SongItem> songs)
        {
            LoggerMembers.VmLogger.Info("正在创建并替换播放源");
            MediaPlaybackList mpl = new MediaPlaybackList();
            foreach (var song in songs)
                AddItem(mpl, song);
            App.Player.Source = mpl;
            LoggerMembers.VmLogger.Info("完成播放源替换");
        }

        public void PushToNext(SongItem song)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
            {
                LoggerMembers.VmLogger.Info("正在添加歌曲至当前播放歌曲的下方");
                if (mpl.Items.Contains(song.PlaybackItem))
                    mpl.Items.Remove(song.PlaybackItem);

                mpl.Items.Insert((int) mpl.CurrentItemIndex + 1, song.PlaybackItem);
                LoggerMembers.VmLogger.Info("添加完成");
            }
            else
                Push(new[] {song});
        }

        public void Append(SongItem song)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
            {
                LoggerMembers.VmLogger.Info("正在添加该歌曲至最后");
                AddItem(mpl, song);
                LoggerMembers.VmLogger.Info("完成歌曲添加");
            }
            else
                Push(new[] {song});
        }

        public void Append(IEnumerable<SongItem> songs)
        {
            if (App.Player.Source is MediaPlaybackList mpl)
            {
                LoggerMembers.VmLogger.Info("正在添加该歌曲组至最后");
                foreach (var song in songs)
                    AddItem(mpl, song);
                LoggerMembers.VmLogger.Info("完成歌曲组添加");
            }
            else
                Push(songs);
        }
    }
}