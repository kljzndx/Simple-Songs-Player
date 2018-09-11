using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
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

        public PlayingPage()
        {
            this.InitializeComponent();
            vm = DataContext as PlayingViewModel;

            PlayItemChangeNotifier.ItemChanged += PlayItemChangeNotifier_ItemChanged;
            PositionChangeNotifier.PositionChanged += PositionChangeNotifier_PositionChanged;
        }

        private void SetLyricSource(LyricBlock lyric)
        {
            if (lyric == null)
            {
                PreviewArea_ScrollLyricsPreview.Lyrics = null;
                Error_StackPanel.Visibility = Visibility.Visible;
                PickLyric_Button.Visibility = vm.AllLyricBlocks.Any() ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                PreviewArea_ScrollLyricsPreview.Lyrics = lyric.Lines;
                Error_StackPanel.Visibility = Visibility.Collapsed;
            }
            PreviewArea_ScrollLyricsPreview.Reposition(TimeSpan.Zero);
        }

        private void SetLyricSource()
        {
            LyricBlock lyric = vm.AllLyricBlocks.Find(l => l.FileName.Contains(vm.CurrentSong.Title));
            SetLyricSource(lyric);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<Song, List<LyricBlock>> tuple)
            {
                vm.CurrentSong = tuple.Item1;
                vm.AllLyricBlocks = tuple.Item2;
            }
            else throw new Exception("未传入歌词列表");

            SetLyricSource();
        }

        private void PlayItemChangeNotifier_ItemChanged(object sender, PlayItemChangeEventArgs e)
        {
            vm.CurrentSong = e.Song;
            SetLyricSource();
        }

        private void PositionChangeNotifier_PositionChanged(object sender, PositionChangeEventArgs e)
        {
            if (e.DidUserChange)
                PreviewArea_ScrollLyricsPreview.Reposition(e.Position);
            else
                PreviewArea_ScrollLyricsPreview.RefreshLyric(e.Position);
        }

        private void LyricsFiles_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var lyricBlock = e.AddedItems.First() as LyricBlock;
            if (lyricBlock == null)
                return;

            SetLyricSource(lyricBlock);
        }
    }
}
