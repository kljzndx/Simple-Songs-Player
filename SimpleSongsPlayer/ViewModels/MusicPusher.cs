using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media.Playback;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.DataServers;
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

            await PlaybackListDataServer.Current.Push(music);

            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        public static async Task Push(IEnumerable<MusicFileDTO> files)
        {
            OtherSettingProperties.Current.CanOptionNowPlayList = false;

            await PlaybackListDataServer.Current.SetUp(files);

            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        public static async Task PushToNext(MusicFileDTO file)
        {
            OtherSettingProperties.Current.CanOptionNowPlayList = false;

            await PlaybackListDataServer.Current.PushToNext(file);

            OtherSettingProperties.Current.CanOptionNowPlayList = true;
        }

        public static async Task Append(MusicFileDTO file) => await Append(new[] {file});

        public static async Task Append(IEnumerable<MusicFileDTO> files)
        {
            OtherSettingProperties.Current.CanOptionNowPlayList = false;

            await PlaybackListDataServer.Current.Append(files);

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