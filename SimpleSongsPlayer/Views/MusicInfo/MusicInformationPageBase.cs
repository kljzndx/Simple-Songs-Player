using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.Views.Controllers;

namespace SimpleSongsPlayer.Views.MusicInfo
{
    public class MusicInformationPageBase : Page
    {
        protected MusicInfoViewModel ViewModel;
        private bool needRefreshLyric;

        public MusicInformationPageBase()
        {
            CustomMediaPlayerElement.PositionChanged += CustomMediaPlayerElement_PositionChanged;
            NavigationCacheMode = NavigationCacheMode.Enabled;

            LyricFileDataServer.Current.DataAdded += LyricFileDataServer_DataAdded;
            LyricFileDataServer.Current.DataRemoved += LyricFileDataServer_DataRemoved;
        }

        protected void Init(MusicInfoViewModel vm)
        {
            this.ViewModel = vm;

            vm.PropertyChanged += Vm_PropertyChanged;

            GetControlByName<ListView>("LyricFile_ListView").ItemsSource = LyricFileDataServer.Current.Data;
            GetControlByName<TextBlock>("NoFile_TextBlock").Visibility =
                LyricFileDataServer.Current.Data.Any() ? Visibility.Collapsed : Visibility.Visible;
        }

        private T GetControlByName<T>(string name)
        {
            var ctol = FindName(name);
            if (ctol == null)
                throw new ArgumentException($"No found control, Name: {name}");
            if (!(ctol is T))
                throw new ArgumentException($"Control Type error, Name: {name}");

            return (T) ctol;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.Init();

            GetControlByName<Grid>("LyricFileSelector_Grid").Visibility = Visibility.Collapsed;
            GetControlByName<Grid>("LyricPreview_Grid").Visibility = Visibility.Visible;

            var slfb = GetControlByName<Button>("SetUpLyricFile_Button");
            slfb.Visibility = LyricIndexDataServer.Current.Data.Any() ? Visibility.Visible : Visibility.Collapsed;
            slfb.IsEnabled = ViewModel.MusicSource != null;
        }

        private async void CustomMediaPlayerElement_PositionChanged(CustomMediaPlayerElement sender, PlayerPositionChangeEventArgs args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var slp = GetControlByName<ScrollLyricsPreviewControl>("My_ScrollLyricsPreviewControl");
                if (args.IsUser || needRefreshLyric)
                {
                    needRefreshLyric = false;
                    slp.Reposition(args.Position);
                }
                else
                    slp.RefreshLyric(args.Position);
            });
        }

        private void LyricFileDataServer_DataAdded(object sender, IEnumerable<LyricFileDTO> e)
        {
            GetControlByName<TextBlock>("NoFile_TextBlock").Visibility = Visibility.Collapsed;
        }

        private void LyricFileDataServer_DataRemoved(object sender, IEnumerable<LyricFileDTO> e)
        {
            if (!LyricFileDataServer.Current.Data.Any())
                GetControlByName<TextBlock>("NoFile_TextBlock").Visibility = Visibility.Collapsed;
        }

        protected void My_ScrollLyricsPreviewControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            CustomMediaPlayerElement.SetPosition_Global(((LyricLine)e.ClickedItem).Time);
        }

        private void Vm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ViewModel.LyricSource):
                    needRefreshLyric = true;
                    break;
                case nameof(ViewModel.MusicSource):
                    GetControlByName<Button>("SetUpLyricFile_Button").IsEnabled = ViewModel.MusicSource != null;
                    break;
            }
        }

        protected void SetUpLyricFile_Button_OnClick(object sender, RoutedEventArgs e)
        {
            GetControlByName<Grid>("LyricPreview_Grid").Visibility = Visibility.Collapsed;
            GetControlByName<Grid>("LyricFileSelector_Grid").Visibility = Visibility.Visible;
            var search = GetControlByName<TextBox>("Search_TextBox");
            search.Text = String.Empty;

            search.Focus(FocusState.Keyboard);
        }

        protected void Search_TextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var lf = GetControlByName<ListView>("LyricFile_ListView");
            var search = GetControlByName<TextBox>("Search_TextBox");
            if (!String.IsNullOrWhiteSpace(search.Text))
            {
                var data = LyricFileDataServer.Current.Data.Where(f => f.FileName.Contains(search.Text)).ToList();
                lf.ItemsSource = data;
            }
            else
                lf.ItemsSource = LyricFileDataServer.Current.Data;
        }

        protected void Submit_Button_OnClick(object sender, RoutedEventArgs e)
        {
            var lf = GetControlByName<ListView>("LyricFile_ListView");
            LyricFileDTO lyric = lf.SelectedItem as LyricFileDTO;
            if (ViewModel.MusicSource != null && lyric != null)
                LyricIndexDataServer.Current.SetIndex(ViewModel.MusicSource.FilePath, lyric.FilePath);

            GetControlByName<Grid>("LyricFileSelector_Grid").Visibility = Visibility.Collapsed;
            GetControlByName<Grid>("LyricPreview_Grid").Visibility = Visibility.Visible;
        }

    }
}