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
using SimpleSongsPlayer.ViewModels.Extensions;
using SimpleSongsPlayer.Views.Controllers;
using SimpleSongsPlayer.Views.MusicInfo;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [PageTitle("MusicInfoPage")]
    public sealed partial class MusicInformationPage : Page
    {
        private readonly ApplicationView _currentView = ApplicationView.GetForCurrentView();

        public MusicInformationPage()
        {
            this.InitializeComponent();
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationViewMode") ||
                !ApiInformation.IsEnumNamedValuePresent(typeof(ApplicationViewMode).FullName, "CompactOverlay") ||
                !_currentView.IsViewModeSupported(ApplicationViewMode.CompactOverlay))
            {
                PinOption_StackPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void AutoScale()
        {
            if (ActualWidth > 800)
                Main_Frame.NavigateEx(typeof(MusicInformationLongPage));
            else
                Main_Frame.NavigateEx(typeof(MusicInformationSmallPage));
            Main_Frame.BackStack.Clear();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            AutoScale();
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
