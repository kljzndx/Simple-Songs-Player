using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.SettingProperties;
using SimpleSongsPlayer.ViewModels.SideViews;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.SidePages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [PageTitle("SettingsPage")]
    public sealed partial class SettingsPage : Page
    {
        private SettingLocator locator = SettingLocator.Current;
        private SettingsViewModel vm;

        public SettingsPage()
        {
            this.InitializeComponent();
            vm = (SettingsViewModel) DataContext;
        }
        
        private void SetupLibrary_Button_OnClick(object sender, RoutedEventArgs e)
        {
            SetupLibrary_ContentDialog.ShowAsync();
        }

        private async void SetupLibrary_ContentDialog_OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            if (vm.MusicLibrary != null)
                return;
            vm.MusicLibrary = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            vm.MusicLibrary.DefinitionChanged += MusicLibrary_DefinitionChanged;
        }

        private async void MusicLibrary_DefinitionChanged(StorageLibrary sender, object args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                await MusicLibraryDataServer.Current.ScanMusicFiles();
                await LyricFileDataServer.Current.ScanFile();
            });
        }

        private async void AddLibrary_Button_OnClick(object sender, RoutedEventArgs e)
        {
            await vm.MusicLibrary.RequestAddFolderAsync();
        }

        private async void LibraryFolders_ListView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var folder = (StorageFolder) e.ClickedItem;
            await vm.MusicLibrary.RequestRemoveFolderAsync(folder);
        }

        private void TimedExit_ToggleSwitch_OnToggled(object sender, RoutedEventArgs e)
        {
            if (TimedExit_ToggleSwitch.IsOn)
            {
                if (locator.Other.TimedExitMinutes < 15)
                    locator.Other.TimedExitMinutes = 15;

                FrameworkPage.Current.TimedExitTimer = ThreadPoolTimer.CreatePeriodicTimer(
                    async t =>
                    {
                        await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            locator.Other.TimedExitMinutes -= 1;

                            if ((int) locator.Other.TimedExitMinutes == 0)
                            {
                                t.Cancel();
                                locator.Other.IsTimedExitEnable = false;
                                if (App.MediaPlayer.PlaybackSession is MediaPlaybackSession session && session.PlaybackState == MediaPlaybackState.Playing)
                                    App.MediaPlayer.Pause();
                                App.Current.Exit();
                            }
                        });
                    }, TimeSpan.FromMinutes(1));
            }
            else
                FrameworkPage.Current.TimedExitTimer?.Cancel();
        }
    }
}
