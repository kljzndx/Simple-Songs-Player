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
        public event ItemClickEventHandler ItemClick;

        private async void FavoritesDialog_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (manager is null)
                manager = await PlayingListManager.GetManager();

            List<FavoriteItem> favoriteItems = new List<FavoriteItem>();
            var blocks = manager.GetBlocks();
            foreach (var block in blocks)
                favoriteItems.Add(await FavoriteItem.FromPlayingListBlock(block));

            Main_ListView.ItemsSource = favoriteItems;
        }

        private void Main_ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            this.Hide();
            ItemClick?.Invoke(sender, e);
        }

        private void Add_ListViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
            RequestAdd?.Invoke(this, EventArgs.Empty);
        }
    }
}
