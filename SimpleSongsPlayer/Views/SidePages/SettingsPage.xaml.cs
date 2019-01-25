using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.System;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using SimpleSongsPlayer.Service;
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
        public const string timedExitTaskName = "TimedExitTask";

        private SettingLocator locator = SettingLocator.Current;
        private SettingsViewModel vm;

        public SettingsPage()
        {
            this.InitializeComponent();
            vm = (SettingsViewModel) DataContext;
        }

        private async Task<bool> StartTimer(uint minutes)
        {
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(t => t.Name is timedExitTaskName);
            task?.Unregister(true);

            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                Name = timedExitTaskName,
            };

            var b = await BackgroundExecutionManager.RequestAccessAsync();
            if (b == BackgroundAccessStatus.Unspecified)
            {
                await MessageBox.ShowAsync("Cannot create timed Task", "Please Open Background permissions for this app in the Windows Settings --> privacy --> Background App",
                    new Dictionary<string, UICommandInvokedHandler>
                    {
                        { "Open Settings", async u => await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-backgroundapps")) }
                    }, "Close");
                return false;
            }

            if (b == BackgroundAccessStatus.DeniedBySystemPolicy || b == BackgroundAccessStatus.DeniedByUser)
            {
                await MessageBox.ShowAsync("Cannot create background task", "Close");
                return false;
            }

            builder.SetTrigger(new TimeTrigger(minutes, true));
            builder.Register();

            return true;
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
                    await MusicLibraryFileServiceManager.Current.ScanFiles();
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

        private async void TimedExitMinutes_ComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int minutes = (int) e.AddedItems.First();
            if (minutes == 0)
            {
                var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(t => t.Name is timedExitTaskName);
                task?.Unregister(true);
                return;
            }

            var dateTime = DateTime.Now.AddMinutes(minutes);
            TimedExitTime_Run.Text = dateTime.ToString("hh:mm:ss");

            var success = await StartTimer((uint) minutes);
            if (!success)
                TimedExitMinutes_ComboBox.SelectedIndex = 0;
        }
    }
}
