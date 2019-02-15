using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Pickers;
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
using Newtonsoft.Json.Linq;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels;
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
        public static DateTimeOffset ExitTime;

        private SettingLocator locator = SettingLocator.Current;
        private SettingsViewModel vm;

        public SettingsPage()
        {
            this.InitializeComponent();
            vm = (SettingsViewModel) DataContext;
        }

        private async Task<bool> StartTimer()
        {
            var task = BackgroundTaskRegistration.AllTasks.Values.FirstOrDefault(t => t.Name is timedExitTaskName);
            if (task != null)
            {
                this.LogByObject("定时退出任务已创建");
                return true;
            }

            this.LogByObject("开始创建定时退出任务");
            BackgroundTaskBuilder builder = new BackgroundTaskBuilder
            {
                Name = timedExitTaskName,
            };

            this.LogByObject("申请注册任务");
            var b = await BackgroundExecutionManager.RequestAccessAsync();
            if (b == BackgroundAccessStatus.Unspecified)
            {
                this.LogByObject("申请失败，原因：没权限");
                await MessageBox.ShowAsync("Cannot create timed Task", "Please Open Background permissions for this app in the Windows Settings --> privacy --> Background App",
                    new Dictionary<string, UICommandInvokedHandler>
                    {
                        { "Open Settings", async u => await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-backgroundapps")) }
                    }, "Close");
                return false;
            }

            if (b == BackgroundAccessStatus.DeniedBySystemPolicy || b == BackgroundAccessStatus.DeniedByUser)
            {
                this.LogByObject("申请失败，原因：省电方案");
                await MessageBox.ShowAsync("Cannot create background task", "Close");
                return false;
            }

            this.LogByObject($"申请成功，正在设置 15 分钟循环定时");
            builder.SetTrigger(new TimeTrigger(15, false));
            this.LogByObject("注册定时退出任务");
            builder.Register();

            this.LogByObject("注册完成");
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
                if (task != null)
                {
                    this.LogByObject("取消定时退出任务");
                    task.Unregister(true);
                }
                return;
            }

            ExitTime = DateTime.Now.AddMinutes(minutes - 10);
            TimedExitTime_Run.Text = ExitTime.AddMinutes(10).ToString("hh:mm:ss");

            var success = await StartTimer();
            if (!success)
                TimedExitMinutes_ComboBox.SelectedIndex = 0;
        }

        private async void LeadingOutFavorites_Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (FavoritesDataServer.Current.Data.Count <= 0)
            {
                await MessageBox.ShowAsync(ResourceLoader.GetForCurrentView("SettingsPage").GetString("LeadingOutFavorites_ErrorInfo"), "OK");
                return;
            }

            var picker = new FileSavePicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeChoices.Add("Json file", new List<string>{".json"});
            var file = await picker.PickSaveFileAsync();
            if (file is null)
                return;

            string json = FavoritesDataServer.Current.ToJson();

            await FileIO.WriteTextAsync(file, json);
        }

        private async void LeadingInFavorites_Button_OnClick(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.SuggestedStartLocation = PickerLocationId.MusicLibrary;
            picker.FileTypeFilter.Add(".json");
            var file = await picker.PickSingleFileAsync();
            if (file is null)
                return;
            string content = await FileIO.ReadTextAsync(file);
            JArray root = JArray.Parse(content);
            foreach (var token in root)
            {
                var favorites = token.Value<JObject>();
                string name = null;
                List<string> paths = null;

                {
                    if (favorites.TryGetValue("Name", out var nameToken))
                        name = nameToken.Value<string>();

                    if (favorites.TryGetValue("Paths", out var pathsToken))
                        paths = pathsToken.Values<string>().ToList();
                }

                if (name != null && paths != null)
                    await FavoritesDataServer.Current.FavoriteOption.AddRange(name, paths);
                else
                    throw new Exception("Cannot parse this file");
            }
        }
    }
}
