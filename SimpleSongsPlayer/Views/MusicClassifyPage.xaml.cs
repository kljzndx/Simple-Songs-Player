using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Arguments;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
using SimpleSongsPlayer.ViewModels.SettingProperties;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicClassifyPage : Page
    {
        private static readonly Type ListPageType = typeof(MusicListPage);
        private static readonly Type GroupPageType = typeof(MusicGroupListPage);

        public MusicClassifyPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        
        private void Root_Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Root_Pivot.SelectedIndex)
            {
                case 0:
                    Song_Frame.NavigateEx(ListPageType, new MusicListArguments(MusicLibraryDataServer.Current.MusicFilesList));
                    break;
                case 1:
                    Artist_Frame.NavigateEx(GroupPageType,
                        ValueTuple.Create(MusicLibraryDataServer.Current.MusicFilesList,
                            new MusicGrouperArgs(new MusicArtistGrouper(), new MusicArtistFilter())));
                    break;
                case 2:
                    Album_Frame.NavigateEx(GroupPageType,
                        ValueTuple.Create(MusicLibraryDataServer.Current.MusicFilesList,
                            new MusicGrouperArgs(new MusicAlbumGrouper(), new MusicAlbumFilter())));
                    break;
                case 3:
                    Favorites_Frame.NavigateEx(GroupPageType, FavoritesDataServer.Current.UserFavoritesList);
                    break;
                case 4:
                    NowPlaying_Frame.NavigateEx(typeof(MusicListPage), new MusicListArguments(NowPlayingDataServer.Current.DataSource, new [] {new MusicListMenuItem("MoreMenu_Remove", NowPlaying_RemoveItem_Click)}));
                    break;
            }
        }

        private async void AllMusicClassifyPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            await MusicLibraryDataServer.Current.ScanMusicFiles();
            await FavoritesDataServer.Current.InitializeFavoritesService();
            if (!OtherSettingProperties.Current.IsMigratedOldFavorites)
            {
                await FavoritesDataServer.Current.MigrateOldFavorites();
                OtherSettingProperties.Current.IsMigratedOldFavorites = true;
            }
        }

        private void MusicGroup_Frame_OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (e.SourcePageType != ListPageType)
                return;

            e.Cancel = true;
            Frame.Navigate(e.SourcePageType, e.Parameter);
        }

        private async Task NowPlaying_RemoveItem_Click(MusicFileDynamic source)
        {
            if (App.MediaPlayer.Source is MediaPlaybackList mpl)
                mpl.Items.Remove(await source.Original.GetPlaybackItem());
        }
    }
}
