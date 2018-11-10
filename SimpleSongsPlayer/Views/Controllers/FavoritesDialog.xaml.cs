using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
using NLog;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Operator;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class FavoritesDialog : ContentDialog
    {
        private PlayingListManager manager;

        public FavoritesDialog()
        {
            this.InitializeComponent();
            this.Opened += FavoritesDialog_Opened;
        }

        public event TypedEventHandler<FavoritesDialog, EventArgs> RequestAdd;
        public event TypedEventHandler<FavoritesDialog, FavoriteItem> ItemClick;

        private async void FavoritesDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            LoggerMembers.PagesLogger.Info("正在初始化数据");
            if (manager is null)
                manager = await PlayingListManager.GetManager();

            List<FavoriteItem> favoriteItems = new List<FavoriteItem>();
            LoggerMembers.PagesLogger.Info("正在获取所有播放列表");
            var blocks = manager.GetBlocks();
            LoggerMembers.PagesLogger.Info("加载数据中");
            foreach (var block in blocks)
                favoriteItems.Add(await FavoriteItem.FromPlayingListBlock(block));

            LoggerMembers.PagesLogger.Info("正在生成 UI");
            Main_ListView.ItemsSource = favoriteItems;
            LoggerMembers.PagesLogger.Info("完成 UI生成");
        }

        private void Add_ListViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();

            LoggerMembers.PagesLogger.Info("点击按钮 新建播放列表");
            RequestAdd?.Invoke(this, EventArgs.Empty);
        }

        private void Main_ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            this.Hide();

            LoggerMembers.PagesLogger.Info("用户点击了一个播放列表");
            ItemClick?.Invoke(this, e.ClickedItem as FavoriteItem);
        }
    }
}
