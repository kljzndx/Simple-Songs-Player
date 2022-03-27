using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.EntityFrameworkCore;

using Nito.AsyncEx;

using SimpleSongsPlayer.Dal;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Views.Controllers;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SimpleSongsPlayer.Services
{
    public class PlaybackListManageService
    {
        private MainDbContext DbContext => Ioc.Default.GetRequiredService<MainDbContext>();
        private MusicFileManageService _manageService;
        private MediaPlaybackList _playbackList;
        private bool _isInit = false;

        private AsyncLock _lock = new AsyncLock();

        public PlaybackListManageService(ConfigurationService configService, MusicFileManageService manageService)
        {
            _manageService = manageService;

            _playbackList = new MediaPlaybackList()
            {
                AutoRepeatEnabled = true,
                ShuffleEnabled = configService.LoopingMode == LoopingModeEnum.Random,
            };
            _playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
        }

        public PlaybackItem CurrentPlayItem { get; private set; }

        public void NotifyPlayItemChange(PlaybackItem source)
        {
            CurrentPlayItem = source;
            WeakReferenceMessenger.Default.Send("CurrentPlayChanged", nameof(PlaybackListManageService));
        }

        public async Task InitPlayList()
        {
            if (!DbContext.PlaybackList.Any())
                return;

            var dbPlayList = DbContext.PlaybackList.Include(pi => pi.File).OrderBy(pi => pi.TrackId).ToList();
            var muiList = dbPlayList.Select(pi => new MusicUi(pi.File)).ToList();

            var removeList = new List<MusicUi>();

            await LoadFilesAsync(async () =>
            {
                foreach (var mui in muiList)
                {
                    try
                    {
                        _playbackList.Items.Add(await mui.GetPlaybackItem());
                    }
                    catch (Exception)
                    {
                        removeList.Add(mui);
                        continue;
                    }
                }
            });

            await _manageService.RemoveMusicData(removeList);

            var playing = dbPlayList.FirstOrDefault(pi => pi.IsPlaying);
            if (playing != null)
            {
                if (playing.TrackId != 0)
                    _playbackList.MoveTo((uint)playing.TrackId);

                NotifyPlayItemChange(playing);
            }

            _isInit = true;
        }

        public MediaPlaybackList GetPlaybackList()
        {
            return _playbackList;
        }

        public async Task PushImmediately(MusicUi source)
        {
            if (source.IsPlaying)
                return;

            var item = await PushToRelative(source, -1);
            if (item != null)
                _playbackList.MoveTo((uint) item.TrackId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="interpolation">相对于当前播放项的距离</param>
        /// <returns></returns>
        public async Task<PlaybackItem> PushToRelative(MusicUi source, int interpolation)
        {
            if (source.IsPlaying)
                return null;

            if (!DbContext.PlaybackList.Any())
            {
                await PushGroup(new[] { source });
                return null;
            }

            MediaPlaybackItem playItem;

            try
            {
                playItem = await source.GetPlaybackItem();
            }
            catch (Exception)
            {
                await _manageService.RemoveMusicData(new[] { source });
                return null;
            }

            var dbPlayList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
            int trackId = (int)_playbackList.CurrentItemIndex;

            var dbPlayItem = await DbContext.PlaybackList.FirstOrDefaultAsync(pi => pi.MusicFileId == source.Id);
            if (dbPlayItem != null)
            {
                _playbackList.Items.Remove(_playbackList.Items[dbPlayItem.TrackId]);
                dbPlayList.Remove(dbPlayItem);
            }

            int targetId = trackId + interpolation;
            if (targetId < 0)
                targetId = 0;
            else if (targetId > dbPlayList.Count)
                targetId = dbPlayList.Count;

            var newItem = dbPlayItem == null ? new PlaybackItem(source.Id) : dbPlayItem;
            dbPlayList.Insert(targetId, newItem);
            SetUpTrackId(dbPlayList, targetId);

            DbContext.PlaybackList.UpdateRange(dbPlayList);
            await DbContext.SaveChangesAsync();

            _playbackList.Items.Insert(targetId, playItem);
            return newItem;
        }

        public async Task PushGroup(IEnumerable<MusicUi> source, MusicUi needPlayItem = null)
        {
            var sourceList = source.ToList();

            bool isEqual = sourceList.Count == DbContext.PlaybackList.Count();
            if (isEqual)
            {
                var dbDataList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
                for (int i = 0; i < sourceList.Count; i++)
                {
                    if (sourceList[i].Id != dbDataList[i].MusicFileId)
                    {
                        isEqual = false;
                        break;
                    }

                    if (i > 10)
                        break;
                }
            }

            if (!isEqual)
            {
                var dbPlayList = new List<PlaybackItem>();
                var playList = new List<MediaPlaybackItem>();
                var removeList = new List<MusicUi>();

                await LoadFilesAsync(async () =>
                {
                    for (int i = 0; i < sourceList.Count; i++)
                    {
                        var item = sourceList[i];
                        try
                        {
                            playList.Add(await item.GetPlaybackItem());
                            dbPlayList.Add(new PlaybackItem(item.Id, i));
                        }
                        catch (Exception)
                        {
                            removeList.Add(item);
                            continue;
                        }
                    }
                });

                var pi = dbPlayList.FirstOrDefault();
                if (pi != null)
                    pi.IsPlaying = true;

                removeList.ForEach(item => sourceList.Remove(item));
                await _manageService.RemoveMusicData(removeList);

                CleanDbAndList();
                DbContext.PlaybackList.AddRange(dbPlayList);
                await DbContext.SaveChangesAsync();

                playList.ForEach(_playbackList.Items.Add);
            }

            if (!DbContext.PlaybackList.Any())
                return;

            var npi = needPlayItem ?? sourceList.First();
            var p = DbContext.PlaybackList.FirstOrDefault(pi => pi.MusicFileId == npi.Id);
            if (p != null && p.TrackId != 0)
                _playbackList.MoveTo((uint) p.TrackId);
            else
                NotifyPlayItemChange(DbContext.PlaybackList.First(pi => pi.TrackId == 0));
        }

        public async Task Append(IEnumerable<MusicUi> source)
        {
            var dbPlayList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
            if (!dbPlayList.Any())
            {
                await PushGroup(source);
                return;
            }

            var sourceList = source.Where(mu => dbPlayList.All(pi => pi.MusicFileId != mu.Id)).ToList();
            if (!sourceList.Any())
                return;

            var playList = new List<MediaPlaybackItem>();
            var removeList = new List<MusicUi>();

            int lastId = dbPlayList.Count - 1;

            await LoadFilesAsync(async () =>
            {
                foreach (var mui in sourceList)
                {
                    try
                    {
                        playList.Add(await mui.GetPlaybackItem());
                        dbPlayList.Add(new PlaybackItem(mui.Id));
                    }
                    catch (Exception)
                    {
                        removeList.Add(mui);
                        continue;
                    }
                }
            });

            await _manageService.RemoveMusicData(removeList);

            SetUpTrackId(dbPlayList, lastId);
            DbContext.UpdateRange(dbPlayList);
            await DbContext.SaveChangesAsync();

            playList.ForEach(_playbackList.Items.Add);
        }

        private async Task LoadFilesAsync(Func<Task> action)
        {
            using (await _lock.LockAsync())
            {
                WeakReferenceMessenger.Default.Send("FilesLoading", nameof(PlaybackListManageService));
                Ioc.Default.GetRequiredService<FlyoutNotification>().Show("FilesLoading");
                await action();
                Ioc.Default.GetRequiredService<FlyoutNotification>().Hide();
                WeakReferenceMessenger.Default.Send("FilesLoaded", nameof(PlaybackListManageService));
            }
        }

        private void SetUpTrackId(List<PlaybackItem> playList, int startId = 0)
        {
            for (int i = startId; i < playList.Count; i++)
                playList[i].TrackId = i;
        }

        private void CleanDbAndList()
        {
            DbContext.PlaybackList.RemoveRange(DbContext.PlaybackList);
            _playbackList.Items.Clear();
        }

        private async void PlaybackList_CurrentItemChanged(MediaPlaybackList sender, CurrentMediaPlaybackItemChangedEventArgs args)
        {
            await Ioc.Default.GetRequiredService<CoreDispatcher>().RunAsync(CoreDispatcherPriority.Normal,
            async () =>
            {
                if (!_isInit) return;

                int trackId = (int)sender.CurrentItemIndex;
                if (trackId < 0 || trackId >= await DbContext.PlaybackList.CountAsync()) return;

                var dbPlayList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
                dbPlayList.ForEach(pi => pi.IsPlaying = pi.TrackId == trackId);

                DbContext.PlaybackList.UpdateRange(dbPlayList);
                await DbContext.SaveChangesAsync();
                NotifyPlayItemChange(dbPlayList[trackId]);
            });
        }
    }
}
