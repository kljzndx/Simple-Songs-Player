﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// Source: https://docs.microsoft.com/windows/uwp/launch-resume/create-a-customized-splash-screen
    /// </summary>
    public sealed partial class LoadingPage : Page
    {
        private readonly LoadingViewModel vm;

        public LoadingPage()
        {
            InitializeComponent();
            vm = this.DataContext as LoadingViewModel;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            FadeIn_Storyboard.Begin();
            Information_TextBlock.Text = "正在扫描文件夹";
            await vm.ScanFolders(new List<StorageFolder> {KnownFolders.MusicLibrary});
            Information_TextBlock.Text = "扫描完成";
            FadeOut_Storyboard.Begin();
        }

        private void FadeOut_Storyboard_OnCompleted(object sender, object e)
        {
            (List<Song> allSongs, List<LyricBlock> allLyricBlocks) tuple = (vm.AllSongs, vm.AllLyricBlocks);
            Frame.Navigate(typeof(MainPage), tuple);
        }
    }
}