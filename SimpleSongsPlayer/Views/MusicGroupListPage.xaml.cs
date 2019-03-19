using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Log;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Arguments;
using SimpleSongsPlayer.ViewModels.DataServers;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.SettingProperties;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicGroupListPage : Page
    {
        private MusicGroupListViewModel vm;
        private MusicGroupViewSettingProperties settings = MusicGroupViewSettingProperties.Current;
        private readonly OtherSettingProperties otherSettings = OtherSettingProperties.Current;

        private List<MusicItemMenuItem<MusicFileDynamic>> itemExtraMenu;
        private readonly List<MusicItemMenuItem<MusicFileGroup>> groupMenu = new List<MusicItemMenuItem<MusicFileGroup>>();

        public MusicGroupListPage()
        {
            groupMenu.Add(new MusicItemMenuItem<MusicFileGroup>("MoreMenu_AddNowPlaying", async g => await MusicPusher.Append(g.Items)));
            groupMenu.Add(new MusicItemMenuItem<MusicFileGroup>("MoreMenu_Favorite", async g => FavoriteAdditionNotification.RequestFavoriteAddition(g.Items)));
            Resources["GroupMoreMenu"] = groupMenu;

            this.InitializeComponent();
            vm = (MusicGroupListViewModel) DataContext;
        }

        private IEnumerable<MusicFileDTO> GetSongItems(IEnumerable<MusicFileGroup> groups)
        {
            this.LogByObject("正在提取出所有的音乐");
            var data = groups.Select(g => g.Items).Aggregate((l, r) =>
            {
                var d = new ObservableCollection<MusicFileDTO>(l);
                foreach (var musicFileDto in r)
                    d.Add(musicFileDto);
                return d;
            });
            return data;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MusicGroupArguments args)
            {
                if (args.ArgsType.HasFlag(MusicGroupArgsType.GroupMenu))
                {
                    groupMenu.AddRange(args.ExtraGroupMenu);

                    {
                        foreach (var menuItem in args.ExtraGroupMenu)
                        {
                            AppBarButton more = new AppBarButton();
                            more.Label = menuItem.Name;
                            more.Click += async (s, ea) =>
                            {
                                foreach (MusicFileGroup item in Main_GridView.SelectedItems.ToList())
                                    if (menuItem.Action != null)
                                        await menuItem.Action.Invoke(item);
                            };
                            more.SetBinding(AppBarButton.IsEnabledProperty, new Binding{Source = OtherSettingProperties.Current, Path = new PropertyPath("CanOptionNowPlayList"), Mode = BindingMode.OneWay});

                            GroupsOptions_CommandBar.SecondaryCommands.Add(more);
                        }
                    }
                }
                if (args.ArgsType.HasFlag(MusicGroupArgsType.ItemMenu))
                    itemExtraMenu = args.ExtraItemMenu;
                if (args.ArgsType.HasFlag(MusicGroupArgsType.DataServer))
                    args.DataServer.DataAdded += DataServer_DataAdded;

                switch (args.ArgsType & (~MusicGroupArgsType.GroupMenu) & (~MusicGroupArgsType.ItemMenu) & (~MusicGroupArgsType.DataServer))
                {
                    case MusicGroupArgsType.GroupSource:
                        vm.SetUp(args.GroupSource);
                        break;
                    case MusicGroupArgsType.ItemSource | MusicGroupArgsType.Grouper:
                        vm.SetUp(args.ItemSource, args.GrouperArgs);
                        break;
                }
            }
        }

        private void MusicGroupListPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            ListSorter_ListBox.SelectedIndex = (int)settings.SortMethod;
        }

        private void Main_GridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicFileGroup;
            if (item is null || Main_GridView.SelectionMode == ListViewSelectionMode.Multiple)
                return;

            if (vm.ItemFilter != null)
                if (itemExtraMenu != null)
                    Frame.Navigate(typeof(MusicListPage), new MusicListArguments(source:vm.Original, dataServer:MusicFileDataServer.Current, filter:new MusicFilterArgs(vm.ItemFilter, item.Name), extraMenu:itemExtraMenu, title: item.Name));
                else
                    Frame.Navigate(typeof(MusicListPage), new MusicListArguments(source:vm.Original, dataServer:MusicFileDataServer.Current, filter:new MusicFilterArgs(vm.ItemFilter, item.Name), title: item.Name));
            else if (itemExtraMenu != null)
                Frame.Navigate(typeof(MusicListPage), new MusicListArguments(source:item.Items, extraMenu:itemExtraMenu, title:item.Name));
            else
                Frame.Navigate(typeof(MusicListPage), new MusicListArguments(source:item.Items, title:item.Name));
        }

        private async void PlayAll_Button_OnClick(object sender, RoutedEventArgs e)
        {
            if (vm.DataSource.Any())
                await MusicPusher.Push(GetSongItems(vm.DataSource));
        }

        private void DataServer_DataAdded(object sender, IEnumerable<KeyValuePair<MusicFileGroup, IEnumerable<MusicFileDTO>>> e)
        {
            foreach (var pair in e)
            {
                if (pair.Value.Count() >= pair.Key.Items.Count)
                {
                    vm.AutoSort();
                    break;
                }
            }
        }

        private void ListSorter_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isEqual = (int) settings.SortMethod == ListSorter_ListBox.SelectedIndex;
            settings.SortMethod =
                (MusicGroupSorterMembers) ListSorter_ListBox.SelectedIndex;

            if (isEqual)
                vm.AutoSort();
            else
                vm.Sort(e.AddedItems.Cast<MusicSorterUi<MusicFileGroup>>().First());
        }

        private void ReverseItems_Button_Click(object sender, RoutedEventArgs e)
        {
            vm.Reverse();
            settings.IsReverse = !settings.IsReverse;
        }

        private void SwitchMultipleSelection_FloatingActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            Main_GridView.SelectionMode = ListViewSelectionMode.Multiple;
            settings.IsSingleMode = false;
            SwitchMultipleSelection_FloatingActionButton.Visibility = Visibility.Collapsed;
            SwitchSingleSelection_FloatingActionButton.Visibility = Visibility.Visible;
            SwitchSingleSelection_FloatingActionButton.Focus(FocusState.Pointer);
        }

        private void SwitchSingleSelection_FloatingActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            Main_GridView.SelectionMode = ListViewSelectionMode.Single;
            settings.IsSingleMode = true;
            SwitchSingleSelection_FloatingActionButton.Visibility = Visibility.Collapsed;
            SwitchMultipleSelection_FloatingActionButton.Visibility = Visibility.Visible;
            SwitchMultipleSelection_FloatingActionButton.Focus(FocusState.Pointer);
        }

        private void SelectAll_AppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Main_GridView.SelectAll();
        }

        private async void PlaySelectedItems_AppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            await MusicPusher.Push(GetSongItems(Main_GridView.SelectedItems.Cast<MusicFileGroup>()));
        }

        private async void AddSelectedItemsToNowPlaying_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            await MusicPusher.Append(GetSongItems(Main_GridView.SelectedItems.Cast<MusicFileGroup>()));
        }

        private void AddSelectedItemsToFavorites_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            FavoriteAdditionNotification.RequestFavoriteAddition(GetSongItems(Main_GridView.SelectedItems.Cast<MusicFileGroup>()));
        }

        private async void MusicGroupItemTemplate_OnPlayRequested(object sender, RoutedEventArgs e)
        {
            var theButton = sender as Control;
            var group = theButton?.DataContext as MusicFileGroup;
            if (group is null)
                return;

            theButton.IsEnabled = false;

            await MusicPusher.Push(group.Items);

            theButton.IsEnabled = true;
        }

        private void Search_AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (String.IsNullOrWhiteSpace(sender.Text))
            {
                sender.ItemsSource = null;
                return;
            }

            sender.ItemsSource = vm.DataSource.Where(g => g.Name.ToLower().Contains(sender.Text.ToLower()));
        }

        private void Search_AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var source = args.SelectedItem as MusicFileGroup;
            if (source is null)
                return;

            sender.Text = source.Name;

            Main_GridView.SelectedItem = source;
            Main_GridView.ScrollIntoView(source, ScrollIntoViewAlignment.Leading);
        }
    }
}
