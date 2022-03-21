using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.EntityFrameworkCore;

using SimpleSongsPlayer.Dal;
using SimpleSongsPlayer.Models;

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

        public async Task Push(MusicUi source)
        {
            if (source.IsPlaying)
                return;

            var data = await DbContext.PlaybackList.FirstOrDefaultAsync(pi => pi.MusicFileId == source.Id);
            if (data != null)
            {
                _playbackList.MoveTo((uint) data.TrackId);
            }
            else
            {
                MediaPlaybackItem playbackItem;

                try
                {
                    playbackItem = await source.GetPlaybackItem();
                }
                catch (Exception)
                {
                    await _manageService.RemoveMusicData(new [] {source});
                    return;
                }

                CleanDbAndList();
                var playItem = new PlaybackItem(source.Id);
                playItem.IsPlaying = true;

                DbContext.PlaybackList.Add(playItem);
                await DbContext.SaveChangesAsync();

                _playbackList.Items.Add(playbackItem);
                NotifyPlayItemChange(playItem);
            }
        }

        public async Task PushGroup(IEnumerable<MusicUi> source)
        {
            CleanDbAndList();
            var sourceList = source.ToList();
            var dbPlayList = new List<PlaybackItem>();
            var playList = new List<MediaPlaybackItem>();
            var removeList = new List<MusicUi>();

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

            var pi = dbPlayList.FirstOrDefault();
            if (pi != null)
                pi.IsPlaying = true;

            await _manageService.RemoveMusicData(removeList);

            DbContext.PlaybackList.AddRange(dbPlayList);
            await DbContext.SaveChangesAsync();

            playList.ForEach(_playbackList.Items.Add);
            if (playList.Any())
                NotifyPlayItemChange(dbPlayList.First());
        }

        public async Task PushToNext(MusicUi source)
        {
            if (source.IsPlaying)
                return;

            if (!DbContext.PlaybackList.Any())
            {
                await Push(source);
                return;
            }

            MediaPlaybackItem playItem;

            try
            {
                playItem = await source.GetPlaybackItem();
            }
            catch (Exception)
            {
                await _manageService.RemoveMusicData(new[] { source });
                return;
            }

            var dbPlayList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
            int trackId = (int)_playbackList.CurrentItemIndex;

            var dbPlayItem = await DbContext.PlaybackList.FirstOrDefaultAsync(pi => pi.MusicFileId == source.Id);
            if (dbPlayItem != null)
            {
                _playbackList.Items.Remove(_playbackList.Items[dbPlayItem.TrackId]);
                dbPlayList.Remove(dbPlayItem);
            }

            var newItem = dbPlayItem == null ? new PlaybackItem(source.Id) : dbPlayItem;
            dbPlayList.Insert(trackId + 1, newItem);
            SetUpTrackId(dbPlayList, trackId);

            DbContext.PlaybackList.UpdateRange(dbPlayList);
            await DbContext.SaveChangesAsync();

            _playbackList.Items.Insert(trackId + 1, playItem);
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

            await _manageService.RemoveMusicData(removeList);

            SetUpTrackId(dbPlayList, lastId);
            DbContext.UpdateRange(dbPlayList);
            await DbContext.SaveChangesAsync();

            playList.ForEach(_playbackList.Items.Add);
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
