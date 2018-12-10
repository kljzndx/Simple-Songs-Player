using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Templates
{
    public sealed partial class MusicFileItemTemplate : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(MusicFileDynamic), typeof(MusicFileItemTemplate), new PropertyMetadata(null));

        public static readonly DependencyProperty MoreFlyoutProperty = DependencyProperty.Register(
            nameof(MoreFlyout), typeof(FlyoutBase), typeof(MusicFileItemTemplate), new PropertyMetadata(null));

        public MusicFileItemTemplate()
        {
            this.InitializeComponent();
        }

        public MusicFileDynamic Source
        {
            get => (MusicFileDynamic) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public FlyoutBase MoreFlyout
        {
            get => (FlyoutBase) GetValue(MoreFlyoutProperty);
            set => SetValue(MoreFlyoutProperty, value);
        }

        public event RoutedEventHandler PlayButton_Click;

        private void Play_Button_OnClick(object sender, RoutedEventArgs e)
        {
            PlayButton_Click?.Invoke(this, e);
        }
    }
}
