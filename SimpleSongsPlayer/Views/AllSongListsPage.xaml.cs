using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.Attributes.Getters;
using SimpleSongsPlayer.Views.SideViews;
using SimpleSongsPlayer.Views.SongViews;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AllSongListsPage : Page
    {
        public static AllSongListsPage Current;

        private readonly AllSongListsViewModel vm;
        private readonly MenuFlyout more_MenuFlyout;

        public AllSongListsPage()
        {
            more_MenuFlyout = new MenuFlyout();
            this.Resources["MoreMenuFlyout"] = more_MenuFlyout;

            this.InitializeComponent();
            vm = this.DataContext as AllSongListsViewModel;
            Current = this;

            LoggerMembers.PagesLogger.Info("开始初始化 “更多” 菜单");
            LoggerMembers.PagesLogger.Info("开始创建 “设置” 菜单项");
            more_MenuFlyout.Items.Add(CreateMenuItemFromPageType(typeof(SettingsPage)));
            LoggerMembers.PagesLogger.Info("开始创建 “关于” 菜单项");
            more_MenuFlyout.Items.Add(CreateMenuItemFromPageType(typeof(AboutPage)));
            LoggerMembers.PagesLogger.Info("完成初始化 “更多” 菜单");
        }

        public MenuFlyoutItemBase CreateMenuItemFromPageType(Type pageType)
        {
            LoggerMembers.PagesLogger.Info("正在获取页面标题");
            string name = SideViewNameGetter.GetNameFromType(pageType);
            LoggerMembers.PagesLogger.Info("正在初始化菜单项");
            var item = new MenuFlyoutItem {Text = name, Tag = pageType};
            item.Click += More_MenuFlyoutItem_Click;
            LoggerMembers.PagesLogger.Info("完成菜单项初始化");
            return item;
        }

        public void OpenSideView(Type pageType)
        {
            LoggerMembers.PagesLogger.Info("正在手动打开侧边栏并跳转到指定页面");
            Root_SplitView.IsPaneOpen = true;
            if (SideView_Frame.SourcePageType != pageType)
                SideView_Frame.Navigate(pageType);
            LoggerMembers.PagesLogger.Info("完成侧边栏打开和跳转操作");
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
        
        private void Root_SplitView_OnPaneClosed(SplitView sender, object args)
        {
            LoggerMembers.PagesLogger.Info("已关闭侧面板");
        }

        private void SideView_Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            LoggerMembers.PagesLogger.Info("正在获取页面标题");
            Title_TextBlock.Text = SideViewNameGetter.GetNameFromType(e.SourcePageType);
            LoggerMembers.PagesLogger.Info("完成页面标题获取");
        }

        private void More_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem item && item.Tag is Type pageType)
            {
                Root_SplitView.IsPaneOpen = true;
                LoggerMembers.PagesLogger.Info("已打开侧面板");
                if (SideView_Frame.SourcePageType != pageType)
                {
                    LoggerMembers.PagesLogger.Info($"正在切换至 {item.Text} 视图");
                    SideView_Frame.Navigate(pageType);
                }
                LoggerMembers.PagesLogger.Info($"已切换至 {item.Text} 视图");
            }
        }
    }
}
