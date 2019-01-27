﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
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
using SimpleSongsPlayer.ViewModels.Attributes.Getters;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
using SimpleSongsPlayer.ViewModels.SettingProperties;
using SimpleSongsPlayer.Views.Controllers;
using SimpleSongsPlayer.Views.SidePages;

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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            foreach (var item in MoreButton_MenuFlyout.Items)
            {
                var menuItem = item as MenuFlyoutItem;
                if (menuItem is null)
                    continue;

                string[] resourceInfo = menuItem.Tag.ToString().Split('/');
                menuItem.Text = ResourceLoader.GetForCurrentView(resourceInfo[0]).GetString(resourceInfo[1]);
            }
        }

        private void Root_Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Root_Pivot.SelectedIndex)
            {
                case 0:
                    Song_Frame.NavigateEx(ListPageType, new MusicListArguments(MusicFileDataServer.Current.Data, MusicFileDataServer.Current));
                    break;
                case 1:
                    Artist_Frame.NavigateEx(GroupPageType, new MusicGroupArguments(itemSource:MusicFileDataServer.Current.Data, grouperArgs:new MusicGrouperArgs(new MusicArtistGrouper(), new MusicArtistFilter())));
                    break;
                case 2:
                    Album_Frame.NavigateEx(GroupPageType, new MusicGroupArguments(itemSource:MusicFileDataServer.Current.Data, grouperArgs:new MusicGrouperArgs(new MusicAlbumGrouper(), new MusicAlbumFilter())));
                    break;
                case 3:
                    Favorites_Frame.NavigateEx(GroupPageType,
                        new MusicGroupArguments(dataServer: FavoritesDataServer.Current,
                            groupSource: FavoritesDataServer.Current.Data,
                            extraGroupMenu: new[]
                            {
                                new MusicItemMenuItem<MusicFileGroupDynamic>("MusicListPage", "MoreMenu_Remove",
                                    async g => await FavoritesDataServer.Current.FavoriteOption.RemoveGroup(g.Name)),
                            },
                            extraItemMenu: new[]
                            {
                                new MusicItemMenuItem<MusicFileDynamic>("MusicListPage", "MoreMenu_Remove",
                                    async mf => await FavoritesDataServer.Current.FavoriteOption.RemoveRange(
                                        FrameworkPage.Current.PageMoreInfo, new[] {mf.Original.FilePath})),
                            }));
                    break;
                case 4:
                    NowPlaying_Frame.NavigateEx(typeof(MusicListPage), new MusicListArguments(NowPlayingDataServer.Current.Data, MusicFileDataServer.Current, new [] {new MusicItemMenuItem<MusicFileDynamic>("MusicListPage", "MoreMenu_Remove", NowPlaying_RemoveItem_Click)}));
                    break;
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

        private void Beside_Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            Title_TextBlock.Text = PageTitleGetter.GetTitle(e.SourcePageType);
        }

        private void Settings_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;
            Beside_Frame.NavigateEx(typeof(SettingsPage), null);
        }

        private void About_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            Root_SplitView.IsPaneOpen = !Root_SplitView.IsPaneOpen;
            Beside_Frame.NavigateEx(typeof(AboutPage), null);
        }
    }
}
