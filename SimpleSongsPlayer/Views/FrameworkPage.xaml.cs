using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.Views.SongViews;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameworkPage : Page
    {
        private readonly FrameworkViewModel vm;

        public FrameworkPage()
        {
            this.InitializeComponent();
            vm = this.DataContext as FrameworkViewModel;
            Close_Button.Visibility = Visibility.Collapsed;

            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            PlayItemChangeNotifier.ItemChanged += PlayItemChangeNotifier_ItemChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<List<Song>, List<LyricBlock>> tuple)
            {
                vm.AllSongs = tuple.Item1;
                vm.AllLyricBlocks = tuple.Item2;
            }
            else
                throw new Exception("未传入资源");

            Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
            PlayerController.AllSongs = vm.AllSongs;
        }

        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if (Main_Frame.SourcePageType != typeof(AllSongListsPage))
            {
                e.Handled = true;
                Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
            }
        }

        private void PlayItemChangeNotifier_ItemChanged(object sender, PlayItemChangeEventArgs e)
        {
            vm.CurrentSong = e.Song;
        }

        private void PlayerController_CoverClick(object sender, RoutedEventArgs e)
        {
            if (vm.CurrentSong is null)
                return;

            if (Main_Frame.SourcePageType.Name == typeof(PlayingPage).Name)
            {
                Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
                return;
            }

            ValueTuple<Song, List<LyricBlock>> tuple = ValueTuple.Create(vm.CurrentSong, vm.AllLyricBlocks);
            Main_Frame.Navigate(typeof(PlayingPage), tuple);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
        }

        private void Main_Frame_Navigated(object sender, NavigationEventArgs e)
        {
            Close_Button.Visibility = e.SourcePageType != typeof(AllSongListsPage) ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
