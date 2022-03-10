using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using Microsoft.EntityFrameworkCore;

using SimpleSongsPlayer.Dal;
using SimpleSongsPlayer.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace SimpleSongsPlayer.Services
{
    public class PlaybackListManageService
    {
        private MainDbContext DbContext => Ioc.Default.GetRequiredService<MainDbContext>();
        private MediaPlaybackList _playbackList;

        public PlaybackListManageService()
        {
            _playbackList = new MediaPlaybackList();

            _playbackList.CurrentItemChanged += PlaybackList_CurrentItemChanged;
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
                CleanDbAndList();
                DbContext.PlaybackList.Add(new PlaybackItem(source.Id));
                await DbContext.SaveChangesAsync();

                _playbackList.Items.Add(await source.GetPlaybackItem());
                _playbackList.MoveTo(0);
            }
        }

        public async Task PushGroup(IEnumerable<MusicUi> source)
        {
            CleanDbAndList();
            var sourceList = source.ToList();
            var dbPlayList = new List<PlaybackItem>();
            var playList = new List<MediaPlaybackItem>();

            for (int i = 0; i < sourceList.Count; i++)
            {
                var item = sourceList[i];
                playList.Add(await item.GetPlaybackItem());
                dbPlayList.Add(new PlaybackItem(item.Id, i));
            }

            DbContext.PlaybackList.AddRange(dbPlayList);
            await DbContext.SaveChangesAsync();

            playList.ForEach(_playbackList.Items.Add);
            _playbackList.MoveTo(0);
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

            var playList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
            int currentTrackId = (int)_playbackList.CurrentItemIndex;

            var playItem = await DbContext.PlaybackList.FirstOrDefaultAsync(pi => pi.MusicFileId == source.Id);
            if (playItem != null)
            {
                _playbackList.Items.Remove(_playbackList.Items[playItem.TrackId]);
                playList.Remove(playItem);
            }

            var newItem = playItem == null ? new PlaybackItem(source.Id) : playItem;
            playList.Insert(currentTrackId + 1, newItem);
            SetUpTrackId(playList, currentTrackId);

            DbContext.PlaybackList.UpdateRange(playList);
            await DbContext.SaveChangesAsync();

            _playbackList.Items.Insert(currentTrackId + 1, await source.GetPlaybackItem());
        }

        public async Task Append(IEnumerable<MusicUi> source)
        {
            var sourceList = source.ToList();
            var dbPlayList = DbContext.PlaybackList.OrderBy(pi => pi.TrackId).ToList();
            var playList = new List<MediaPlaybackItem>();

            int lastId = dbPlayList.Count - 1;

            foreach (var mui in sourceList)
            {
                playList.Add(await mui.GetPlaybackItem());
                dbPlayList.Add(new PlaybackItem(mui.Id));
            }

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
                var dbPlayList = DbContext.PlaybackList.ToList();
                dbPlayList.ForEach(pi => pi.IsPlaying = pi.TrackId == sender.CurrentItemIndex);

                DbContext.PlaybackList.UpdateRange(dbPlayList);
                await DbContext.SaveChangesAsync();
                WeakReferenceMessenger.Default.Send($"CurrentPlay: {dbPlayList[(int)sender.CurrentItemIndex].MusicFileId}", "MediaPlayer");
            });
        }
    }
}
