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
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.Views.Controllers;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [PageTitle("MusicInfoPage")]
    public sealed partial class MusicInformationPage : Page
    {
        private MusicInfoViewModel vm;
        private bool isRefreshLyric;

        public MusicInformationPage()
        {
            this.InitializeComponent();
            vm = DataContext as MusicInfoViewModel;
            vm.PropertyChanged += Vm_PropertyChanged;
            CustomMediaPlayerElement.PositionChanged += CustomMediaPlayerElement_PositionChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await vm.Init();

            LyricFileSelector_Grid.Visibility = Visibility.Collapsed;
            SetUpLyricFile_Button.Visibility = LyricIndexDataServer.Current.Data.Any() ? Visibility.Visible : Visibility.Collapsed;
            SetUpLyricFile_Button.IsEnabled = vm.MusicSource != null;
        }

        private void CustomMediaPlayerElement_PositionChanged(CustomMediaPlayerElement sender, PlayerPositionChangeEventArgs args)
        {
            if (args.IsUser || isRefreshLyric)
            {
                isRefreshLyric = false;
                My_ScrollLyricsPreviewControl.Reposition(args.Position);
            }
            else
                My_ScrollLyricsPreviewControl.RefreshLyric(args.Position);
        }

        private void My_ScrollLyricsPreviewControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            CustomMediaPlayerElement.SetPosition_Global(((LyricLine) e.ClickedItem).Time);
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(vm.LyricSource):
                    isRefreshLyric = true;
                    break;
                case nameof(vm.MusicSource):
                    SetUpLyricFile_Button.IsEnabled = vm.MusicSource != null;
                    break;
            }
        }

        private void SetUpLyricFile_Button_OnClick(object sender, RoutedEventArgs e)
        {
            LyricPreview_Grid.Visibility = Visibility.Collapsed;
            LyricFileSelector_Grid.Visibility = Visibility.Visible;
            Search_TextBox.Text = String.Empty;
            LyricFile_ListView.ItemsSource = LyricFileDataServer.Current.Data;

            Search_TextBox.Focus(FocusState.Keyboard);
        }

        private void Search_TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(Search_TextBox.Text))
            {
                var data = LyricFileDataServer.Current.Data.Where(f => f.FileName.Contains(Search_TextBox.Text)).ToList();
                LyricFile_ListView.ItemsSource = data;
            }
            else
                LyricFile_ListView.ItemsSource = LyricFileDataServer.Current.Data;
        }

        private void Submit_Button_OnClick(object sender, RoutedEventArgs e)
        {
            LyricFileDTO lyric = LyricFile_ListView.SelectedItem as LyricFileDTO;
            if (vm.MusicSource != null && lyric != null)
                LyricIndexDataServer.Current.SetIndex(vm.MusicSource.FilePath, lyric.FilePath);

            LyricFileSelector_Grid.Visibility = Visibility.Collapsed;
            LyricPreview_Grid.Visibility = Visibility.Visible;
        }
    }
}
