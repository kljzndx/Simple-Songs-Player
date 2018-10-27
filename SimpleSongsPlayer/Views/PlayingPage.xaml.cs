using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Streaming.Adaptive;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Events;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayingPage : Page
    {
        private PlayingViewModel vm;

        private bool isNeedReposition;

        public PlayingPage()
        {
            this.InitializeComponent();
            vm = DataContext as PlayingViewModel;
            NavigationCacheMode = NavigationCacheMode.Enabled;

            PlayItemChangeNotifier.ItemChanged += PlayItemChangeNotifier_ItemChanged;
            PositionChangeNotifier.PositionChanged += PositionChangeNotifier_PositionChanged;
        }

        private void SetLyricSource(LyricBlock lyric)
        {
            if (lyric != null)
            {
                LoggerMembers.PagesLogger.Info("已找到对应歌词， 正在应用歌词");

                PreviewArea_ScrollLyricsPreview.Lyrics = lyric.Lines;
                Error_StackPanel.Visibility = Visibility.Collapsed;
                isNeedReposition = true;

                LoggerMembers.PagesLogger.Info("成功应用歌词");
            }
            else
            {
                LoggerMembers.PagesLogger.Info("未找到对应歌词， 正在显示未找到歌词说明");

                PreviewArea_ScrollLyricsPreview.Lyrics = null;
                Error_StackPanel.Visibility = Visibility.Visible;
                PickLyric_Button.Visibility = vm.AllLyricBlocks.Any() ? Visibility.Visible : Visibility.Collapsed;

                LoggerMembers.PagesLogger.Info("完成说明显示");
            }
        }

        private void SetLyricSource()
        {
            LoggerMembers.PagesLogger.Info("正在查找对应歌词");
            LyricBlock lyric = vm.AllLyricBlocks.Find(l => l.FileName.Contains(vm.CurrentSong.Title));
            SetLyricSource(lyric);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            LoggerMembers.PagesLogger.Info("已切换到播放详情页面， 正在校验数据");

            if (e.Parameter is ValueTuple<Song, List<LyricBlock>> tuple)
            {
                vm.CurrentSong = tuple.Item1;
                vm.AllLyricBlocks = tuple.Item2;
            }
            else throw new Exception("未传入歌词列表");

            LoggerMembers.PagesLogger.Info("数据校验成功");

            SetLyricSource();
        }

        private void PlayItemChangeNotifier_ItemChanged(object sender, PlayItemChangeEventArgs e)
        {
            vm.CurrentSong = e.Song;

            LoggerMembers.PagesLogger.Info("已更新歌曲信息");

            SetLyricSource();
        }

        private void PositionChangeNotifier_PositionChanged(object sender, PositionChangeEventArgs e)
        {
            if (e.DidUserChange || isNeedReposition)
            {
                LoggerMembers.PagesLogger.Info("正在重新定位字幕");

                PreviewArea_ScrollLyricsPreview.Reposition(e.Position);
                isNeedReposition = false;

                LoggerMembers.PagesLogger.Info("完成定位");
            }
            else
                PreviewArea_ScrollLyricsPreview.RefreshLyric(e.Position);
        }

        private void LyricsFiles_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lyricBlock = e.AddedItems.First() as LyricBlock;
            if (lyricBlock == null)
                return;

            LoggerMembers.PagesLogger.Info("用户手动选择了歌词");

            SetLyricSource(lyricBlock);
        }

        private void PreviewArea_ScrollLyricsPreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            LyricLine line = e.ClickedItem as LyricLine;
            if (line == null)
                return;

            LoggerMembers.PagesLogger.Info("用户点击了字幕项");

            PreviewArea_ScrollLyricsPreview.Reposition(line.Time);
            App.Player.PlaybackSession.Position = line.Time;

        }
    }
}
