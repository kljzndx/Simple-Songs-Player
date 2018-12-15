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
    public sealed partial class FavoriteItemTemplate : UserControl
    {
        public FavoriteItemTemplate()
        {
            this.InitializeComponent();
            this.DataContextChanged += FavoriteItemTemplate_DataContextChanged;
        }

        private async void FavoriteItemTemplate_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            var group = args.NewValue as MusicFileGroup;
            if (group != null)
                Main_Image.Source = await group.GetCover();
        }
    }
}
