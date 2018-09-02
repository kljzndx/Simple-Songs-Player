﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.SongViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.SongViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SongsViewPage : SongViewsPageBase
    {
        private readonly SongsViewModel vm;

        public SongsViewPage() : base(((ViewModelLocator)Application.Current.Resources["Locator"]).Songs)
        {
            this.InitializeComponent();
            vm = base.GetViewModel<SongsViewModel>();
        }

        private void ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            FrameworkElement args = e.OriginalSource as FrameworkElement;
            if (args?.DataContext is Song theSong)
                vm.SetPlayerSource(theSong, vm.GetSongs("$all$"));
        }
    }
}
