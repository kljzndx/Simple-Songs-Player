using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;
using SimpleSongsPlayer.ViewModels;

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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicListPage : Page
    {
        private MusicListViewModel _vm;

        public MusicListPage()
        {
            this.InitializeComponent();
            _vm = Ioc.Default.GetRequiredService<MusicListViewModel>();
        }

        private async void PlayAll_AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            var list = new List<MusicUi>();
            _vm.Source.Select(mg => mg.Items).ToList().ForEach(list.AddRange);
            await _vm.PlaybackListService.PushGroup(list);
        }

        private async void PlayGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var s = sender as FrameworkElement;
            var context = s.DataContext as MusicGroup;

            await _vm.PlaybackListService.PushGroup(context.Items);
        }

        private void DataSource_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataSource_ListView.SelectedIndex >= 0)
                _vm.ConfigService.DataSourceId = DataSource_ListView.SelectedIndex;
            else
                DataSource_ListView.SelectedIndex = _vm.ConfigService.DataSourceId;
        }
    }
}
