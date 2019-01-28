using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
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
        private bool needRefreshLyric;
        private readonly ApplicationView _currentView = ApplicationView.GetForCurrentView();

        public MusicInformationPage()
        {
            this.InitializeComponent();
            vm = DataContext as MusicInfoViewModel;
            vm.PropertyChanged += Vm_PropertyChanged;
            CustomMediaPlayerElement.PositionChanged += CustomMediaPlayerElement_PositionChanged;
            NavigationCacheMode = NavigationCacheMode.Enabled;

            LyricFile_ListView.ItemsSource = LyricFileDataServer.Current.Data;
            NoFile_TextBlock.Visibility =
                LyricFileDataServer.Current.Data.Any() ? Visibility.Collapsed : Visibility.Visible;
            LyricFileDataServer.Current.DataAdded += LyricFileDataServer_DataAdded;
            LyricFileDataServer.Current.DataRemoved += LyricFileDataServer_DataRemoved;

            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewMode") ||
                !ApiInformation.IsEnumNamedValuePresent(typeof(ApplicationViewMode).FullName, "CompactOverlay") ||
                !_currentView.IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                PinOption_StackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AutoScale()
        {
            MusicInfo_StackPanel.Visibility = ActualHeight > 500 ? Visibility.Visible : Visibility.Collapsed;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await vm.Init();

            LyricFileSelector_Grid.Visibility = Visibility.Collapsed;
            LyricPreview_Grid.Visibility = Visibility.Visible;

            SetUpLyricFile_Button.Visibility = LyricIndexDataServer.Current.Data.Any() ? Visibility.Visible : Visibility.Collapsed;
            SetUpLyricFile_Button.IsEnabled = vm.MusicSource != null;

            AutoScale();
        }

        private async void CustomMediaPlayerElement_PositionChanged(CustomMediaPlayerElement sender, PlayerPositionChangeEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (args.IsUser || needRefreshLyric)
                {
                    needRefreshLyric = false;
                    My_ScrollLyricsPreviewControl.Reposition(args.Position);
                }
                else
                    My_ScrollLyricsPreviewControl.RefreshLyric(args.Position);
            });
        }

        private void LyricFileDataServer_DataAdded(object sender, IEnumerable<LyricFileDTO> e)
        {
            NoFile_TextBlock.Visibility = Visibility.Collapsed;
        }

        private void LyricFileDataServer_DataRemoved(object sender, IEnumerable<LyricFileDTO> e)
        {
            if (!LyricFileDataServer.Current.Data.Any())
                NoFile_TextBlock.Visibility = Visibility.Collapsed;
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
                    needRefreshLyric = true;
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

        private void MusicInformationPage_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            AutoScale();
        }

        private async void Pin_FloatingActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModePreferences preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
            preferences.CustomSize = new Size(500, 500);
            preferences.ViewSizePreference = ViewSizePreference.Custom;
            var isSuccess = await _currentView.TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);

            if (!isSuccess)
                throw new Exception("Can not pin to the screen");

            Pin_FloatingActionButton.Visibility = Visibility.Collapsed;
            UnPin_FloatingActionButton.Visibility = Visibility.Visible;
            UnPin_FloatingActionButton.Focus(FocusState.Pointer);
        }

        private async void UnPin_FloatingActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            await _currentView.TryEnterViewModeAsync(ApplicationViewMode.Default);
            
            UnPin_FloatingActionButton.Visibility = Visibility.Collapsed;
            Pin_FloatingActionButton.Visibility = Visibility.Visible;
            Pin_FloatingActionButton.Focus(FocusState.Pointer);
        }
    }
}
