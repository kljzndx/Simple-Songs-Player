using SimpleSongsPlayer.Models;

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

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers.Templates
{
    public sealed partial class MusicItemTemplate : UserControl
    {
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(MusicUi), typeof(MusicItemTemplate), new PropertyMetadata(null));

        public event EventHandler<RoutedEventArgs> PlayButtonClick;
        public event EventHandler<RoutedEventArgs> MoreButtonClick;

        public MusicItemTemplate()
        {
            this.InitializeComponent();
        }

        public MusicUi Source
        {
            get { return (MusicUi)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private void Play_Button_Click(object sender, RoutedEventArgs e)
        {
            PlayButtonClick?.Invoke(sender, e);
        }

        private void More_Button_Click(object sender, RoutedEventArgs e)
        {
            MoreButtonClick?.Invoke(sender, e);
        }
    }
}
