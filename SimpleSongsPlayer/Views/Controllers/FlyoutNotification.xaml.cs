using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
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
    public sealed partial class FlyoutNotification : UserControl
    {
        public FlyoutNotification()
        {
            this.InitializeComponent();
        }

        public void Show(string message, TimeSpan timeout)
        {
            Show(message);

            if (timeout != TimeSpan.Zero)
                ThreadPoolTimer.CreateTimer(timer => Hide(), timeout);
        }

        public void Show(string message)
        {
            Fold_Storyboard.Stop();
            Message_TextBlock.Text = message;
            Root_Border.Visibility = Visibility.Visible;
            Extend_Storyboard.Begin();
        }

        public void Hide()
        {
            Fold_Storyboard.Begin();
        }

        private void Fold_Storyboard_Completed(object sender, object e)
        {
            Root_Border.Visibility = Visibility.Collapsed;

        }
    }
}
