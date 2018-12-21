using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using NLog.LayoutRenderers;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class FlyoutNotification : UserControl
    {
        private static FlyoutNotification current;

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            nameof(Text), typeof(string), typeof(FlyoutNotification), new PropertyMetadata(String.Empty));

        public FlyoutNotification()
        {
            this.InitializeComponent();
            current = this;
            RefreshRadius();
            this.Visibility = Visibility.Collapsed;
        }

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static void Show(string text)
        {
            if (current.Visibility == Visibility.Visible)
                return;
            current.Visibility = Visibility.Visible;
            current.Text = text;
            current.FadeInTranslate_DoubleAnimation.From = current.ActualWidth;
            current.FadeIn_Storyboard.Begin();
        }

        public static void Hide()
        {
            if (current.Visibility == Visibility.Collapsed)
                return;

            current.FadeOutTranslate_DoubleAnimation.To = current.ActualWidth;
            current.FadeOut_Storyboard.Begin();
        }

        public static void ShowOnce(string text, int sec = 2)
        {
            Show(text);
            ThreadPoolTimer.CreateTimer(async t =>
            {
                await current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Hide);
            }, TimeSpan.FromSeconds(sec));
        }

        private void RefreshRadius()
        {
            var radius = this.ActualHeight / 2;
            var margin = radius;
            Root_Border.CornerRadius = new CornerRadius(radius, 0, 0, radius);
            Main_TextBlock.Margin = new Thickness(margin, 0, 8, 0);
        }
        
        private void FadeOut_Storyboard_OnCompleted(object sender, object e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        private void FlyoutNotification_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            RefreshRadius();
        }
    }
}
