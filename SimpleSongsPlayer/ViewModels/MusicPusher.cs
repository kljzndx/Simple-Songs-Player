using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.SettingProperties;

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
            OtherSettingProperties.Current.CanOptionNowPlayList = false;

            CurrentType.LogByType("获取播放项");
            var playbackItem = await music.GetPlaybackItem();
            
            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
            {
                if (mpl.CurrentItem.Equals(playbackItem))
                    return;

                if (!mpl.Items.Contains(playbackItem))
                {
                    CurrentType.LogByType("将播放项插入至最顶端");
                    mpl.Items.Insert(0, playbackItem);
                }

                CurrentType.LogByType("移动磁头");
                mpl.MoveTo((uint) mpl.Items.IndexOf(playbackItem));
            }
            else
                await Push(new[] {playbackItem});

            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        public static async Task Push(IEnumerable<MusicFileDTO> files)
        {
            OtherSettingProperties.Current.CanOptionNowPlayList = false;

            CurrentType.LogByType("接收文件");
            var source = new Queue<MusicFileDTO>(files);
            CurrentType.LogByType("播放第一首歌");
            var fmpi = await source.Dequeue().GetPlaybackItem();
            await Push(new[] {fmpi});

            await Append(source.ToList());
            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        public static async Task PushToNext(MusicFileDTO file)
        {
            OtherSettingProperties.Current.CanOptionNowPlayList = false;

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
                await Push(new[] {mpi});

            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        public static async Task Append(MusicFileDTO file) => await Append(new[] {file});

        public static async Task Append(IEnumerable<MusicFileDTO> files)
        {
            OtherSettingProperties.Current.CanOptionNowPlayList = false;
            CurrentType.LogByType("正在接收参数");
            var source = new Queue<MusicFileDTO>(files);

            while (source.Any())
            {
                var option = new List<MusicFileDTO>();

                CurrentType.LogByType($"正在提取 10 条项目");

                for (int i = 0; i < 10; i++)
                    if (source.Any())
                        option.Add(source.Dequeue());
                    else
                        break;

                var plList = new List<MediaPlaybackItem>();

                CurrentType.LogByType("解析所有文件");
                foreach (var dto in option)
                    plList.Add(await dto.GetPlaybackItem());

                CurrentType.LogByType("添加到正在播放");
                await Append(plList);
            }
            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        private static async Task Append(IEnumerable<MediaPlaybackItem> items)
        {
            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
            {
                CurrentType.LogByType("正在解析文件列表");
                await Task.Run(() =>
                {
                    foreach (var item in items)
                        AddItem(mpl, item);
                });
            }
            else
                await Push(items);
        }

        private static async Task Push(IEnumerable<MediaPlaybackItem> items)
        {
            CurrentType.LogByType("正在创建播放列表");
            var mpl = new MediaPlaybackList();
            CurrentType.LogByType("正在配置播放模块");
            App.MediaPlayer.Source = mpl;

            CurrentType.LogByType("正在解析传进来的播放项列表");
            await Task.Run(() =>
            {
                foreach (var item in items)
                    AddItem(mpl, item);
            });
        }
    }
}