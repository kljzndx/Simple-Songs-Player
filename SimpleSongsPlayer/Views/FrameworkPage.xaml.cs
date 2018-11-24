using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

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
            this.Loaded += FrameworkPage_Loaded;
        }

        private async void FrameworkPage_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await Task.Run(() => throw new Exception("异步异常"));
        }
    }
}
