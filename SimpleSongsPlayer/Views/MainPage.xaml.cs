using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.Services;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.Views.Controllers;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Info_StackPanel.Children.Insert(0, Ioc.Default.GetRequiredService<FlyoutNotification>());

            WeakReferenceMessenger.Default.Register<MainPage, string, string>(this, nameof(MusicListPage), (page, mes) =>
            {
                if (mes == "GiveCommentButtonClick")
                {
                    Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9N6406PNNRZS"));
                }

                if (mes == "AboutButtonClick")
                {
                    page.About_ContentDialog.ShowAsync();
                }
            });
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Main_Frame.Navigate(typeof(MusicListPage));
            await Ioc.Default.GetRequiredService<PlaybackListManageService>().InitPlayList();
            await Ioc.Default.GetRequiredService<MusicFileScanningService>().ScanAsync();
        }

        private void CustomPlayerElement_CoverButtonClick(object sender, RoutedEventArgs e)
        {
            if (Main_Frame.CurrentSourcePageType == typeof(MusicInfoPage))
                Main_Frame.GoBack();
            else
                Main_Frame.Navigate(typeof(MusicInfoPage));
        }
    }
}
