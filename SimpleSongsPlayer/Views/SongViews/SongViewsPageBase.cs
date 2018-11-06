using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.DataModel.Events;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Operator;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.SongViewModels;
using SimpleSongsPlayer.Views.ItemTemplates;

namespace SimpleSongsPlayer.Views.SongViews
{
    public abstract class SongViewsPageBase : Page
    {
        private static readonly ResourceLoader MenuResource = ResourceLoader.GetForCurrentView("SongItemMenu");

        private readonly SongViewModelBase vmb;
        private readonly MenuFlyout songItemMenu;

        private MenuFlyoutSubItem addTo_MenuItem;

        private PlayingListManager playingListManager;
        
        protected SongItem SongCache;

        protected SongViewsPageBase(SongViewModelBase vm)
        {
            vmb = vm;
            songItemMenu = new MenuFlyout();
            songItemMenu.Closed += (s, e) => SongCache = null;

            Resources = new ResourceDictionary();
            Resources.Add("SongItemMenu", songItemMenu);
            
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected T GetViewModel<T>() where T : SongViewModelBase
        {
            if (vmb.GetType() != typeof(T))
                throw new Exception("提供的vm类型 与 在构造函数里提供的vm的类型 不一致");

            return (T) vmb;
        }

        protected virtual void MenuInit(MenuFlyout menuFlyout, ResourceLoader stringResource)
        {
            List<MenuFlyoutItemBase> songMenuItems = new List<MenuFlyoutItemBase>();
            List<MenuFlyoutItemBase> addToMenuItems = new List<MenuFlyoutItemBase>();

            {
                LoggerMembers.PagesLogger.Info("初始化菜单项 下一首播放");
                MenuFlyoutItem nextPlay_MenuItem = new MenuFlyoutItem();
                nextPlay_MenuItem.Text = stringResource.GetString("NextPlay");
                nextPlay_MenuItem.Tag = nextPlay_MenuItem.Text;
                nextPlay_MenuItem.Click += NextPlay_MenuItem_Click;

                songMenuItems.Add(nextPlay_MenuItem);

                LoggerMembers.PagesLogger.Info("初始化菜单项 添加到");
                addTo_MenuItem = new MenuFlyoutSubItem();
                addTo_MenuItem.Text = stringResource.GetString("AddTo");
                addTo_MenuItem.Tag = addTo_MenuItem.Text;

                songMenuItems.Add(addTo_MenuItem);
            }

            {
                LoggerMembers.PagesLogger.Info("初始化‘正在播放’菜单项并增加至‘添加到’菜单项里");
                MenuFlyoutItem playing = new MenuFlyoutItem();
                playing.Icon = new FontIcon {Glyph = "\uE189"};
                playing.Text = stringResource.GetString("Playing");
                playing.Tag = playing.Text;
                playing.Click += AddTo_Playing_MenuItem_Click;

                addToMenuItems.Add(playing);

                try
                {
                    LoggerMembers.PagesLogger.Info("初始化分隔符并增加至‘添加到’菜单项里");
                    addToMenuItems.Add(new MenuFlyoutSeparator());
                }
                catch (InvalidCastException e)
                {
                    LoggerMembers.PagesLogger.Error(e, "分隔符初始化失败");
                }

                LoggerMembers.PagesLogger.Info("初始化‘新的播放列表’菜单项并增加至‘添加到’菜单项里");
                MenuFlyoutItem newPlayList = new MenuFlyoutItem();
                newPlayList.Icon = new FontIcon {Glyph = "\uE109"};
                newPlayList.Text = stringResource.GetString("NewPlayList");
                newPlayList.Tag = newPlayList.Text;
                newPlayList.Click += AddTo_NewPlayList_MenuItem_Click;

                addToMenuItems.Add(newPlayList);
                
                foreach (var block in playingListManager.GetBlocks())
                {
                    LoggerMembers.PagesLogger.Info($"初始化‘{block.Name}’菜单项并增加至‘添加到’菜单项里");
                    var menuItem = new MenuFlyoutItem();
                    menuItem.Icon = new FontIcon {Glyph = "\uE154" };
                    menuItem.Text = block.Name;
                    menuItem.Tag = block.Name;
                    menuItem.Click += AddTo_PlayingList_MenuItem_Click;

                    addToMenuItems.Add(menuItem);
                }
            }

            LoggerMembers.PagesLogger.Info("开始为‘添加到’菜单项的子项生成 UI");
            foreach (var item in addToMenuItems)
            {
                try
                {
                    addTo_MenuItem.Items.Add(item);
                    LoggerMembers.PagesLogger.Info($"{item.Tag} UI 生成成功");
                }
                catch (Exception e)
                {
                    LoggerMembers.PagesLogger.Error(e, $"{item.Tag} UI 生成失败");
                    App.ShowErrorDialog(e);
                    continue;
                }
            }

            LoggerMembers.PagesLogger.Info("开始为‘歌曲菜单’的子项生成 UI");
            foreach (var item in songMenuItems)
            {
                try
                {
                    menuFlyout.Items.Add(item);
                    LoggerMembers.PagesLogger.Info($"{item.Tag} UI 生成成功");
                }
                catch (Exception e)
                {
                    LoggerMembers.PagesLogger.Error(e, $"{item.Tag} UI 生成失败");
                    App.ShowErrorDialog(e);
                    continue;
                }
            }
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is List<Song> allSongs)
                await vmb.RefreshData(allSongs);
            else
                throw new Exception("未收到歌曲数据");

            LoggerMembers.PagesLogger.Info($"切换歌曲视图至 {e.SourcePageType.Name}");
        
            playingListManager = await PlayingListManager.GetManager();
        
            if (!songItemMenu.Items.Any())
                MenuInit(songItemMenu, MenuResource);

            if (addTo_MenuItem != null)
            {
                playingListManager.BlockCreated -= PlayingListManager_BlockCreated;
                playingListManager.BlockDeleted -= PlayingListManager_BlockDeleted;
                playingListManager.BlockRenamed -= PlayingListManager_BlockRenamed;

                playingListManager.BlockCreated += PlayingListManager_BlockCreated;
                playingListManager.BlockDeleted += PlayingListManager_BlockDeleted;
                playingListManager.BlockRenamed += PlayingListManager_BlockRenamed;
            }

            LoggerMembers.PagesLogger.Info($"歌曲视图数据初始化完成");
        }

        protected void Songs_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var sg in vmb.SongGroups)
                foreach (var item in sg.Items)
                    item.IsSelected = false;

            var song = e.AddedItems.FirstOrDefault() as SongItem;
            if (song is null)
                return;

            song.IsSelected = true;

            LoggerMembers.PagesLogger.Info("选中歌曲");
        }

        public void Songs_ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            FrameworkElement args = e.OriginalSource as FrameworkElement;
            if (args.DataContext is SongItem theSong)
            {
                LoggerMembers.PagesLogger.Info("用户双击了一个歌曲");
                vmb.Push(theSong);
            }
        }

        protected void PlayItem_Button_Tapped(SongItemTemplate sender, EventArgs args)
        {
            LoggerMembers.PagesLogger.Info("点击按钮 播放此歌曲");
            vmb.Push(sender.Source);
        }
        
        protected void SongItemTemplate_MenuOpening(SongItemTemplate sender, EventArgs args)
        {
            SongCache = sender.Source;
            LoggerMembers.PagesLogger.Info("打开歌曲菜单");
        }

        private void NextPlay_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SongCache is null)
                return;

            LoggerMembers.PagesLogger.Info("点击菜单项 下一首播放");

            vmb.PushToNext(SongCache);
        }

        private void AddTo_Playing_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SongCache is null)
                return;

            LoggerMembers.PagesLogger.Info("点击菜单项 添加到 -> 正在播放");

            vmb.Append(SongCache);
        }

        private void AddTo_NewPlayList_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (SongCache is null)
                return;

            LoggerMembers.PagesLogger.Info("点击菜单项 添加到 -> 新的播放列表");

            PlayingListOperationNotifier.RequestAdd(new[] { SongCache });
        }

        private async void AddTo_PlayingList_MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var theMenuItem = sender as MenuFlyoutItem;
            if (theMenuItem is null)
                return;

            LoggerMembers.PagesLogger.Info("点击菜单项 添加到 -> 播放列表");

            var song = SongCache;
            var block = playingListManager.GetBlock(theMenuItem.Text);
            await block.AddPath(song.Path);
        }

        protected void PlayGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as FrameworkElement;
            var group = btn?.DataContext as SongsGroup;
            if (group != null)
            {
                LoggerMembers.PagesLogger.Info("点击按钮 播放该组");
                vmb.Push(group.Items);
            }
        }

        protected void AddGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as FrameworkElement;
            var group = btn?.DataContext as SongsGroup;
            if (group != null)
            {
                LoggerMembers.PagesLogger.Info("点击按钮 添加该组到正在播放列表");
                vmb.Append(group.Items);
            }
        }

        private void PlayingListManager_BlockCreated(PlayingListManager sender, PlayingListBlock args)
        {
            MenuFlyoutItem item = new MenuFlyoutItem { Text = args.Name };
            item.Click += AddTo_PlayingList_MenuItem_Click;
            addTo_MenuItem.Items.Add(item);
        }

        private void PlayingListManager_BlockDeleted(PlayingListManager sender, PlayingListBlock args)
        {
            var item = addTo_MenuItem.Items.FirstOrDefault(m => m is MenuFlyoutItem mi && mi.Text.Equals(args.Name));
            if (item != null)
                addTo_MenuItem.Items.Remove(item);
        }

        private void PlayingListManager_BlockRenamed(PlayingListManager sender, PlayingListBlockRenamedEventArgs args)
        {
            var item = addTo_MenuItem.Items.FirstOrDefault(m => m is MenuFlyoutItem mi && mi.Text.Equals(args.OldName));
            if (item is MenuFlyoutItem mfi)
                mfi.Text = args.NewName;
        }

    }
}