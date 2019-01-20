using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Media.Playback;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Arguments;
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
        public static FrameworkPage Current;
        private static readonly Type ClassifyPageType = typeof(MusicClassifyPage);
        
        private readonly SystemNavigationManager systemNavigationManager = SystemNavigationManager.GetForCurrentView();
        private IEnumerable<string> musicDtoPaths;
        private readonly ObservableCollection<MusicFileGroup> favorites = FavoritesDataServer.Current.Data;

        public FrameworkPage()
        {
            this.InitializeComponent();
            Current = this;
            Main_Frame.Navigate(ClassifyPageType);
            systemNavigationManager.BackRequested += SystemNavigationManager_BackRequested;

            Main_CustomMediaPlayerElement.SetMediaPlayer(App.MediaPlayer);
            FavoriteAdditionNotification.FavoriteAdditionRequested += FavoriteAdditionNotification_FavoriteAdditionRequested;
        }

        public ThreadPoolTimer TimedExitTimer { get; set; }
        public string PageMoreInfo => PageInfo_TextBlock.Text;

        private void Main_Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            var title = PageTitleGetter.TryGetTitle(e.SourcePageType);
            if (!String.IsNullOrWhiteSpace(title))
            {
                TitleBar_Grid.Visibility = Visibility.Visible;
                Back_Button.Visibility = Main_Frame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;
                PageTitle_TextBlock.Text = title;

                var args = e.Parameter as PageArgumentsBase;
                PageInfo_TextBlock.Text = args?.Title ?? String.Empty;
                PageInfo_TextBlock.Visibility = args != null ? Visibility.Visible : Visibility.Collapsed;
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

        //private async void CustomMediaPlayerElement_NowPlaybackItemChanged(CustomMediaPlayerElement sender, PlayerNowPlaybackItemChangeEventArgs args)
        //{
        //    if (args.NewItem is MediaPlaybackItem mpi)
        //    {
        //        this.LogByObject("刷新音乐信息按钮的专辑图");
        //        var properties = mpi.GetDisplayProperties();
        //        var bitmap = new BitmapImage();
        //        bitmap.SetSource(await properties.Thumbnail.OpenReadAsync());
        //        Cover_Image.Visibility = Visibility.Visible;
        //        Cover_Image.Source = bitmap;
        //    }

        //    if (args.NewItem is null || !NowPlayingDataServer.Current.Data.Any())
        //        Cover_Image.Visibility = Visibility.Collapsed;
        //}

        private void FavoriteAdditionNotification_FavoriteAdditionRequested(object sender, IEnumerable<MusicFileDTO> e)
        {
            this.LogByObject("提取出所有的文件路径");
            musicDtoPaths = e.Select(f => f.FilePath);
            My_FavoriteSelectorDialog.ShowAsync();
        }

        private async void My_FavoriteSelectorDialog_OnFavoriteCreateRequested(object sender, EventArgs e)
        {
            var operation = await FavoriteName_InputDialog.ShowAsync();
            if (operation == ContentDialogResult.Primary)
            {
                this.LogByObject("正在创建收藏");
                await FavoritesDataServer.Current.FavoriteOption.AddRange(FavoriteName_InputDialog.Text, musicDtoPaths);
            }
            else
                My_FavoriteSelectorDialog.ShowAsync();
        }

        private async void My_FavoriteSelectorDialog_OnFavoriteSelected(object sender, string e)
        {
            this.LogByObject("正在添加进所选收藏夹");
            await FavoritesDataServer.Current.FavoriteOption.AddRange(e, musicDtoPaths);
        }

        private void Main_CustomMediaPlayerElement_OnCoverButton_Click(object sender, RoutedEventArgs e)
        {
            Main_Frame.NavigateEx(typeof(MusicInformationPage), null);
        }
    }
}
