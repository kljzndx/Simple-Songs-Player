using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class FavoriteSelectorDialog : ContentDialog
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(IEnumerable<MusicFileGroup>), typeof(FavoriteSelectorDialog), new PropertyMetadata(null));

        public FavoriteSelectorDialog()
        {
            this.InitializeComponent();
            this.SecondaryButtonText = ResourceLoader.GetForCurrentView("Dialog").GetString("Close");
        }

        public IEnumerable<MusicFileGroup> Source
        {
            get => (IEnumerable<MusicFileGroup>) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public event EventHandler<EventArgs> FavoriteCreateRequested;
        public event EventHandler<string> FavoriteSelected;

        private void CreateFavorite_ListViewItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            this.Hide();
            FavoriteCreateRequested?.Invoke(this, EventArgs.Empty);
        }

        private void Favorites_ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            this.Hide();
            var click = e.ClickedItem as MusicFileGroup;
            if (click != null)
                FavoriteSelected?.Invoke(this, click.Name);
        }
    }
}
