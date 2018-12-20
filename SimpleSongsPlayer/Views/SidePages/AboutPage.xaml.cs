using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using HappyStudio.UwpToolsLibrary.Auxiliarys;
using HappyStudio.UwpToolsLibrary.Information;
using SimpleSongsPlayer.Service;
using SimpleSongsPlayer.ViewModels.Attributes;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.SidePages
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [PageTitle("AboutPage")]
    public sealed partial class AboutPage : Page
    {
        private readonly string appName = AppInfo.Name;
        private readonly string appVersion = AppInfo.Version;
        private readonly string feedbackEmailTitle = ResourceLoader.GetForCurrentView("AboutPage").GetString("FeedbackEmailTitle");

        public AboutPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        private async void SendFeedback_AppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.LogByObject("点击按钮 发送反馈");
            await EmailEx.SendAsync("kljzndx@outlook.com", $"{appName} {appVersion} {feedbackEmailTitle}", String.Empty);
        }

        private async void GitHub_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.LogByObject("点击按钮 GitHub");
            await Launcher.LaunchUriAsync(new Uri("https://github.com/kljzndx/Simple-Songs-Player"));
        }

        private async void OpenLogsFolder_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.LogByObject("点击按钮 查看日志");
            await Launcher.LaunchFolderAsync(ApplicationData.Current.TemporaryFolder);
        }

        private async void Review_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            this.LogByObject("点击按钮 评论");
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store://review/?ProductId=9N6406PNNRZS"));
        }
    }
}
