using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.Attributes.Getters;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
using SimpleSongsPlayer.Views.Controllers;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameworkPage : Page
    {
        private static readonly Type ClassifyPageType = typeof(MusicClassifyPage);

        private readonly SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
        private IEnumerable<string> musicDtoPaths;
        private ObservableCollection<MusicFileGroup> favorites = FavoritesDataServer.Current.UserFavoritesList;

        public FrameworkPage()
        {
            this.InitializeComponent();
            Main_Frame.Navigate(ClassifyPageType);
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

            Main_CustomMediaPlayerElement.SetMediaPlayer(App.MediaPlayer);
            CustomMediaPlayerElement.NowPlaybackItemChanged += CustomMediaPlayerElement_NowPlaybackItemChanged;
            FavoriteAdditionNotification.FavoriteAdditionRequested += FavoriteAdditionNotification_FavoriteAdditionRequested;
        }

        private void Main_Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            var title = PageTitleGetter.TryGetTitle(e.SourcePageType);
            if (!String.IsNullOrWhiteSpace(title))
            {
                TitleBar_Grid.Visibility = Visibility.Visible;
                Title_TextBlock.Text = title;
                Back_Button.Visibility = Main_Frame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
            }
            else
                TitleBar_Grid.Visibility = Visibility.Collapsed;
        }

        private void SystemNavigationManager_BackRequested(object sender, BackRequestedEventArgs e)
        {
            if (Main_Frame.CanGoBack)
            {
                if (e != null)
                    e.Handled = true;

                Main_Frame.GoBack();
            }
        }

        private void Back_Button_OnClick(object sender, RoutedEventArgs e)
        {
            Main_Frame.GoBack();
        }

        private async void CustomMediaPlayerElement_NowPlaybackItemChanged(CustomMediaPlayerElement sender, PlayerNowPlaybackItemChangeEventArgs args)
        {
            if (args.NewItem is MediaPlaybackItem mpi)
            {
                var properties = mpi.GetDisplayProperties();
                var bitmap = new BitmapImage();
                bitmap.SetSource(await properties.Thumbnail.OpenReadAsync());
                Cover_Image.Visibility = Visibility.Visible;
                Cover_Image.Source = bitmap;
            }
            else
                Cover_Image.Visibility = Visibility.Collapsed;
        }

        private void FavoriteAdditionNotification_FavoriteAdditionRequested(object sender, IEnumerable<MusicFileDTO> e)
        {
            musicDtoPaths = e.Select(f => f.FilePath);
            My_FavoriteSelectorDialog.ShowAsync();
        }

        private async void My_FavoriteSelectorDialog_OnFavoriteCreateRequested(object sender, EventArgs e)
        {
            var operation = await FavoriteName_InputDialog.ShowAsync();
            if (operation == ContentDialogResult.Primary)
                await FavoritesDataServer.Current.FavoriteOption.AddRange(FavoriteName_InputDialog.Text, musicDtoPaths);
            else
                My_FavoriteSelectorDialog.ShowAsync();
        }

        private async void My_FavoriteSelectorDialog_OnFavoriteSelected(object sender, string e)
        {
            await FavoritesDataServer.Current.FavoriteOption.AddRange(e, musicDtoPaths);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Main_Frame.NavigateEx(typeof(MusicInformationPage), null);
        }
    }
}
