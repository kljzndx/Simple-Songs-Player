using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private StorageLibrary musicLibrary;
        private SettingProperties settings = SettingProperties.Current;

        public SettingsPage()
        {
            this.InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            musicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            MusicPaths_ListView.ItemsSource = musicLibrary.Folders;
        }

        private async void OpenMusicsPathsManager_Button_OnClick(object sender, RoutedEventArgs e)
        {
            await MusicPathsManager_ContentDialog.ShowAsync();
        }

        private void ReScan_Button_OnClick(object sender, RoutedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            rootFrame?.Navigate(typeof(LoadingPage));
        }

        private async void AddFolder_Button_OnClick(object sender, RoutedEventArgs e)
        {
            await musicLibrary.RequestAddFolderAsync();
        }

        private async void MusicPaths_ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            await musicLibrary.RequestRemoveFolderAsync((StorageFolder) e.ClickedItem);
        }

        private void TimerPause_ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            if (TimerPause_ToggleSwitch.IsOn)
            {
                FrameworkPage.Current.PauseTimer?.Cancel();
                FrameworkPage.Current.PauseTimer=ThreadPoolTimer.CreatePeriodicTimer(async t =>
                {
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        if (settings.PauseTimeMinutes > 0)
                            settings.PauseTimeMinutes -= 1;
                        else 
                        {
                            if (App.Player.PlaybackSession.PlaybackState != MediaPlaybackState.Paused &&
                                App.Player.PlaybackSession.PlaybackState != MediaPlaybackState.None)
                            {
                                App.Player.Pause();
                            }
                            settings.IsTimerPauseEnable = false;
                            settings.PauseTimeMinutes = 10;
                            t.Cancel();
                        }
                    });
                }, TimeSpan.FromMinutes(1));
            }
            else
            {
                FrameworkPage.Current.PauseTimer?.Cancel();
                settings.PauseTimeMinutes = 10;
            }
        }
    }
}
