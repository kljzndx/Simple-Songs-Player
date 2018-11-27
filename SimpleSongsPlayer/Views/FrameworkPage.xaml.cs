using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameworkPage : Page
    {
        public FrameworkPage()
        {
            this.InitializeComponent();
        }

        private async void FrameworkPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            var dataServer = await DataServer.GetServer();
            DataLoading_ProgressRing.Value = 50;
            dataServer.MusicFilesService.ScanFiles().GetAwaiter()
                .OnCompleted(async () => await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => DataLoading_ProgressRing.Value=100));
            Main_Frame.Navigate(typeof(SongListPage), dataServer.MusicFilesList);
        }
    }
}
