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
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.Views.SongViews;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AllSongListsPage : Page
    {
        private readonly AllSongListsViewModel vm;

        public AllSongListsPage()
        {
            this.InitializeComponent();
            vm = this.DataContext as AllSongListsViewModel;
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is List<Song> allSongs)
                vm.AllSongs = allSongs;
            else
                throw new Exception("未收到歌曲资源");

            Main_Pivot_SelectionChanged(null, null);

            LoggerMembers.PagesLogger.Info("已切换到 AllSongListsPage");
        }

        private void Main_Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllSongs_Frame.Navigate(typeof(Page));
            AllSongArtists_Frame.Navigate(typeof(Page));
            AllSongAlbums_Frame.Navigate(typeof(Page));
            AllSongFolders_Frame.Navigate(typeof(Page));
            PlayingList_Frame.Navigate(typeof(Page));

            switch (Main_Pivot.SelectedIndex)
            {
                case 0:
                    AllSongs_Frame.Navigate(typeof(SongsViewPage), vm.AllSongs);
                    break;
                case 1:
                    AllSongArtists_Frame.Navigate(typeof(SongArtistsViewPage), vm.AllSongs);
                    break;
                case 2:
                    AllSongAlbums_Frame.Navigate(typeof(SongAlbumsViewPage), vm.AllSongs);
                    break;
                case 3:
                    AllSongFolders_Frame.Navigate(typeof(SongsFoldersViewPage), vm.AllSongs);
                    break;
                case 4:
                    PlayingList_Frame.Navigate(typeof(PlayingList_ViewPage), vm.AllSongs);
                    break;
                default:
                    throw new Exception("未找到对应处理器");
            }
        }

        private void Settings_Button_OnClick(object sender, RoutedEventArgs e)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;
            LoggerMembers.PagesLogger.Info("已打开侧面板");
            LoggerMembers.PagesLogger.Info("已切换至设置页面");
        }
        
        private void Root_SplitView_OnPaneClosed(SplitView sender, object args)
        {
            LoggerMembers.PagesLogger.Info("已关闭侧面板");
        }
    }
}
