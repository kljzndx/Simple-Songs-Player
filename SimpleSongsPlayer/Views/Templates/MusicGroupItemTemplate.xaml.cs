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
    public sealed partial class MusicGroupItemTemplate : UserControl
    {
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(MusicFileGroup), typeof(MusicGroupItemTemplate), new PropertyMetadata(null));

        public MusicGroupItemTemplate()
        {
            this.InitializeComponent();
            DataContextChanged += MusicGroupItemTemplate_DataContextChanged;
        }

        public MusicFileGroup Source
        {
            get => (MusicFileGroup) GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }
        
        private async void MusicGroupItemTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (Source != null)
                Cover_Image.Source = await Source.GetCover();
        }
    }
}
