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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class SongInfoShowed : UserControl
    {
        public static readonly DependencyProperty DoShowCoverProperty = DependencyProperty.Register(
            nameof(DoShowCover), typeof(bool), typeof(SongInfoShowed), new PropertyMetadata(true));

        public static readonly DependencyProperty CoverSourceProperty = DependencyProperty.Register(
            nameof(CoverSource), typeof(BitmapSource), typeof(SongInfoShowed), new PropertyMetadata(null));

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(SongInfoShowed), new PropertyMetadata(String.Empty));

        public static readonly DependencyProperty ArtistProperty = DependencyProperty.Register(
            nameof(Artist), typeof(string), typeof(SongInfoShowed), new PropertyMetadata(String.Empty));

        public SongInfoShowed()
        {
            this.InitializeComponent();
        }

        public bool DoShowCover
        {
            get => (bool) GetValue(DoShowCoverProperty);
            set => SetValue(DoShowCoverProperty, value);
        }

        public BitmapSource CoverSource
        {
            get => (BitmapSource) GetValue(CoverSourceProperty);
            set => SetValue(CoverSourceProperty, value);
        }

        public string Title
        {
            get => (string) GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string Artist
        {
            get => (string) GetValue(ArtistProperty);
            set => SetValue(ArtistProperty, value);
        }
    }
}
