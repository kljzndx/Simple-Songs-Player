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
        }

        private void Main_Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AllSongs_Frame.Navigate(typeof(Page));
            AllSongArtists_Frame.Navigate(typeof(Page));
            AllSongAlbums_Frame.Navigate(typeof(Page));

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
                default:
                    throw new Exception("未找到对应处理器");
            }
        }
    }
}
