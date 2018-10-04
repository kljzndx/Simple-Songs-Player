﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.SongViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.SongViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SongAlbumsViewPage : SongViewsPageBase
    {
        private readonly SongAlbumsViewModel vm;

        public SongAlbumsViewPage() : base(((ViewModelLocator)Application.Current.Resources["Locator"]).SongsLocator.SongAlbums)
        {
            this.InitializeComponent();
            vm = base.GetViewModel<SongAlbumsViewModel>();
        }
    }
}
