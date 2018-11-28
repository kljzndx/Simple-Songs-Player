﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SongListPage : Page
    {
        private SongListViewModel vm;

        public SongListPage()
        {
            this.InitializeComponent();
            vm = (SongListViewModel) DataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ObservableCollection<MusicFileDTO> dtos)
                vm.SetUpDataSource(dtos);
            if (e.Parameter is ValueTuple<ObservableCollection<MusicFileDTO>, MusicFilterArgs> fullParameter)
                vm.SetUpDataSource(fullParameter.Item1, fullParameter.Item2);
        }
    }
}
