using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Playback;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;

namespace SimpleSongsPlayer.ViewModels
{
    public static class MusicPusher
    {
        private static readonly Type CurrentType = typeof(MusicPusher);

        private static void AddItem(MediaPlaybackList target, MediaPlaybackItem item)
        {
            if (!target.Items.Contains(item))
            {
                CurrentType.LogByType("添加播放项至播放列表");
                target.Items.Add(item);
            }
        }

        public static async Task Push(MusicFileDTO music)
        {
            CurrentType.LogByType("获取播放项");
            var playbackItem = await music.GetPlaybackItem();
            
            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
            {
                if (mpl.CurrentItem.Equals(playbackItem))
                    return;

                if (mpl.Items.Contains(playbackItem))
                {
                    CurrentType.LogByType("移除列表内相同播放项");
                    mpl.Items.Remove(playbackItem);
                }

                CurrentType.LogByType("将播放项插入至最顶端，并同步移动磁头");
                mpl.Items.Insert(0, playbackItem);
                mpl.MoveTo(0);
            }
            else
                Push(new[] {playbackItem});
        }

        public static async Task Push(IEnumerable<MusicFileDTO> items)
        {
            CurrentType.LogByType("正在提取播放项");
            List<MediaPlaybackItem> list = new List<MediaPlaybackItem>();
            foreach (var file in items)
                list.Add(await file.GetPlaybackItem());
            
            Push(list);
        }

        public static async Task PushToNext(MusicFileDTO file)
        {
            CurrentType.LogByType("获取播放项");
            var mpi = await file.GetPlaybackItem();

            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
            {
                if (mpl.CurrentItem.Equals(mpi))
                    return;

                if (mpl.Items.Contains(mpi))
                {
                    CurrentType.LogByType("移除列表内相同播放项");
                    mpl.Items.Remove(mpi);
                }

                CurrentType.LogByType("将播放项添加至正在播放音乐的下面");
                uint id = mpl.CurrentItemIndex + 1;
                mpl.Items.Insert((int) id, mpi);
            }
            else
                Push(new[] {mpi});
        }

        public static async Task Append(MusicFileDTO file) => await Append(new[] {file});

        public static async Task Append(IEnumerable<MusicFileDTO> files)
        {
            CurrentType.LogByType("正在提取播放项");
            List<MediaPlaybackItem> items = new List<MediaPlaybackItem>();
            foreach (var file in files)
                items.Add(await file.GetPlaybackItem());

            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
                foreach (var item in items)
                    AddItem(mpl, item);
            else
                Push(items);
        }

        private static void Push(IEnumerable<MediaPlaybackItem> items)
        {
            CurrentType.LogByType("正在创建播放列表");
            var mpl = new MediaPlaybackList();
            foreach (var item in items)
                AddItem(mpl, item);

            CurrentType.LogByType("正在配置播放模块");
            App.MediaPlayer.Source = mpl;
        }
    }
}