using CommunityToolkit.Mvvm.DependencyInjection;

using HappyStudio.UwpToolsLibrary.Auxiliarys.Files;
using HappyStudio.UwpToolsLibrary.Information;

using SimpleSongsPlayer.Services;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class UpdateLogDialog : UserControl
    {
        public UpdateLogDialog()
        {
            this.InitializeComponent();
        }

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (Ioc.Default.GetRequiredService<ConfigurationService>().LatestVersion == AppInfo.Version)
                return;

            Root_ReelDialog.Show("更新日志", await FileReader.ReadText(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/UpdateLog.md")), "GBK"));
            //Main_MarkdownTextBlock.Text = await FileReader.ReadText(await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///Assets/UpdateLog.md")), "GBK");

        }

        private void Root_ReelDialog_Closed(object sender, EventArgs e)
        {
            Ioc.Default.GetRequiredService<ConfigurationService>().LatestVersion = AppInfo.Version;
        }
    }
}
