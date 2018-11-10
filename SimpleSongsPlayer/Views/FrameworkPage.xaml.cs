using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Operator;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.Views.Controllers;
using SimpleSongsPlayer.Views.SongViews;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameworkPage : Page
    {
        public static FrameworkPage Current;
        private PlayingListManager playingListManager;

        private readonly FrameworkViewModel vm;
        private List<string> additionPaths;
        
        public ThreadPoolTimer ExitTimer;

        public FrameworkPage()
        {
            this.InitializeComponent();
            Current = this;

            vm = this.DataContext as FrameworkViewModel;
            Close_Button.Visibility = Visibility.Collapsed;
            NavigationCacheMode = NavigationCacheMode.Enabled;

            SystemNavigationManager.GetForCurrentView().BackRequested += System_BackRequested;
            PlayItemChangeNotifier.ItemChanged += PlayItemChangeNotifier_ItemChanged;
            PlayingListOperationNotifier.AdditionRequested += PlayingListOperationNotifier_AdditionRequested;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ValueTuple<List<Song>, List<LyricBlock>> tuple)
            {
                vm.AllSongs = tuple.Item1;
                vm.AllLyricBlocks = tuple.Item2;
                LoggerMembers.PagesLogger.Info("成功传入资源");
            }
            else
                throw new Exception("未传入资源");

            Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
            PlayerController.AllSongs = vm.AllSongs;
            playingListManager = await PlayingListManager.GetManager();

            LoggerMembers.PagesLogger.Info("FrameworkPage 初始化完成");
        }

        private void System_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Main_Frame.SourcePageType != typeof(AllSongListsPage))
            {
                e.Handled = true;
                LoggerMembers.PagesLogger.Info("返回主页--通过标题栏返回键");
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
                LoggerMembers.PagesLogger.Info("返回主页--通过播放控制条上的歌曲详情按钮");
                Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
                return;
            }

            LoggerMembers.PagesLogger.Info("正在切换至歌曲详情页");
            ValueTuple<Song, List<LyricBlock>> tuple = ValueTuple.Create(vm.CurrentSong, vm.AllLyricBlocks);
            Main_Frame.Navigate(typeof(PlayingPage), tuple);
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            LoggerMembers.PagesLogger.Info("返回主页--通过关闭按钮");
            Main_Frame.Navigate(typeof(AllSongListsPage), vm.AllSongs);
        }

        private void Main_Frame_Navigated(object sender, NavigationEventArgs e)
        {
            Close_Button.Visibility = e.SourcePageType != typeof(AllSongListsPage) ? Visibility.Visible : Visibility.Collapsed;
        }

        private void PlayingListOperationNotifier_AdditionRequested(object sender, PlayingListAdditionRequestedEventArgs e)
        {
            LoggerMembers.PagesLogger.Info("取出歌曲数据");
            additionPaths = e.Paths;
            LoggerMembers.PagesLogger.Info("弹出收藏面板");
            FavoriteSelector_Dialog.ShowAsync();
        }

        private async void FavoriteSelector_Dialog_OnRequestAdd(FavoritesDialog sender, EventArgs args)
        {
            LoggerMembers.PagesLogger.Info("弹出新建播放列表对话框");
            await PlayingListAddition_InputDialog.ShowAsync();
        }

        private async void FavoriteSelector_Dialog_OnItemClick(FavoritesDialog sender, FavoriteItem args)
        {
            LoggerMembers.PagesLogger.Info("正在将歌曲添加至选定播放列表");
            await playingListManager.GetBlock(args.Name).AddPaths(additionPaths);
            LoggerMembers.PagesLogger.Info("完成歌曲添加操作");
        }

        private async void PlayingListAddition_InputDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            LoggerMembers.PagesLogger.Info("正在新建歌单");
            await playingListManager.CreateBlockAsync(PlayingListAddition_InputDialog.Text, additionPaths);
            LoggerMembers.PagesLogger.Info("完成歌单创建");
        }

        private void PlayingListAddition_InputDialog_OnSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            LoggerMembers.PagesLogger.Info("取消新建歌单操作");
        }
    }
}
