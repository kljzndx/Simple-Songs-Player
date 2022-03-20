using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Services;
using SimpleSongsPlayer.ViewModels;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
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

            WeakReferenceMessenger.Default.Register<MusicListPage, string, string>(this, nameof(MusicFileScanningService), (page, mes) =>
            {
                if (mes == "Started")
                    page.DisableLayer_Border.Visibility = Visibility.Visible;

                if (mes == "Finished")
                    page.DisableLayer_Border.Visibility = Visibility.Collapsed;
            });
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

        private async void AddDataSource_Button_Click(object sender, RoutedEventArgs e)
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            var folder = await musicLib.RequestAddFolderAsync();
            if (folder == null)
                return;

            await Ioc.Default.GetRequiredService<MusicFileScanningService>().ScanAsync();
        }

        private async void RemoveDataSource_Button_Click(object sender, RoutedEventArgs e)
        {
            var musicLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
            var item = (DataSourceItem)DataSource_ListView.SelectedItem;
            bool isRemoved = await musicLib.RequestRemoveFolderAsync(await StorageFolder.GetFolderFromPathAsync(item.Name));
            if (!isRemoved)
                return;

            DataSource_ListView.SelectedIndex -= 1;
            var list = _vm.DataSourceList.ToList();
            list.Remove(item);
            _vm.DataSourceList = list;

            await Ioc.Default.GetRequiredService<MusicFileScanningService>().ScanAsync();
        }

        private async void MusicItemTemplate_PlayButtonClick(object sender, RoutedEventArgs e)
        {
            var service = Ioc.Default.GetRequiredService<PlaybackListManageService>();
            await service.Push((MusicUi) Data_ListView.SelectedItem);
        }

        private void MusicItemTemplate_MoreButtonClick(object sender, RoutedEventArgs e)
        {
            More_MenuFlyout.ShowAt((FrameworkElement)sender);
        }

        private async void PlayNext_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var service = Ioc.Default.GetRequiredService<PlaybackListManageService>();
            await service.PushToNext((MusicUi)Data_ListView.SelectedItem);
        }

        private async void AddToPlayList_MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var service = Ioc.Default.GetRequiredService<PlaybackListManageService>();
            await service.Append(new[] { (MusicUi)Data_ListView.SelectedItem });
        }

        private void Data_ListView_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var origin = e.OriginalSource as FrameworkElement;
            var dc = origin.DataContext as MusicUi;
            if (origin == null || dc == null)
                return;

            Data_ListView.SelectedItem = dc;
            More_MenuFlyout.ShowAt(origin, e.GetPosition(origin));
        }

        private async void Data_ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dc = (e.OriginalSource as FrameworkElement)?.DataContext as MusicUi;
            if (dc == null)
                return;

            var service = Ioc.Default.GetRequiredService<PlaybackListManageService>();
            await service.Push(dc);
        }
    }
}
