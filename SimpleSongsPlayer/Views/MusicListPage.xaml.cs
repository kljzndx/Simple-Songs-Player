using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Arguments;
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
using SimpleSongsPlayer.ViewModels.SettingProperties;
using SimpleSongsPlayer.Views.Templates;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [PageTitle("MusicListPage")]
    public sealed partial class MusicListPage : Page
    {
        private MusicListViewModel vm;
        private readonly MusicViewSettingProperties settings = MusicViewSettingProperties.Current;
        private readonly OtherSettingProperties otherSettings = OtherSettingProperties.Current;
        private readonly List<MusicItemMenuItem<MusicFileDynamic>> musicItemMenuList;

        public MusicListPage()
        {
            musicItemMenuList = new List<MusicItemMenuItem<MusicFileDynamic>>();
            Resources["MusicItemMenuList"] = musicItemMenuList;

            musicItemMenuList.Add(new MusicItemMenuItem<MusicFileDynamic>("MoreMenu_PlayNext", s => MusicPusher.PushToNext(s.Original)));
            musicItemMenuList.Add(new MusicItemMenuItem<MusicFileDynamic>("MoreMenu_AddNowPlaying", s => MusicPusher.Append(s.Original)));
            musicItemMenuList.Add(new MusicItemMenuItem<MusicFileDynamic>("MoreMenu_Favorite", async f => FavoriteAdditionNotification.RequestFavoriteAddition(new[] {f.Original})));

            this.InitializeComponent();
            vm = (MusicListViewModel) DataContext;
            vm.DataSource.CollectionChanged += DataSource_CollectionChanged;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MusicListArguments args)
            {
                vm.IsEnableViewOption = args.IsEnableViewOption;

                if (args.ArgsType.HasFlag(MusicListArgsType.Menu))
                {
                    musicItemMenuList.AddRange(args.ExtraMenu);

                    foreach (var menuItem in args.ExtraMenu)
                    {
                        var appBarButton = new AppBarButton { Label = menuItem.Name };
                        appBarButton.Click += async (s, a) =>
                        {
                            foreach (MusicFileDynamic item in Main_ListView.SelectedItems.ToList())
                                await menuItem.Action.Invoke(item);
                        };
                        appBarButton.SetBinding(AppBarButton.IsEnabledProperty, new Binding { Source = OtherSettingProperties.Current, Path = new PropertyPath("CanOptionNowPlayList"), Mode = BindingMode.OneWay });

                        ListOption_CommandBar.SecondaryCommands.Add(appBarButton);
                    }
                }

                if (args.ArgsType.HasFlag(MusicListArgsType.DataServer))
                    args.DataServer.DataAdded += DataServer_DataAdded;

                switch (args.ArgsType & (~MusicListArgsType.Menu) & (~MusicListArgsType.DataServer))
                {
                    case MusicListArgsType.Source:
                        vm.SetUpDataSource(args.Source);
                        break;
                    case MusicListArgsType.Source | MusicListArgsType.Filter:
                        vm.SetUpDataSource(args.Source, args.Filter);
                        break;
                }
            }
        }

        private void MusicListPage_OnLoaded(object sender, RoutedEventArgs e)
        {
            Grouper_ListBox.SelectedIndex = (int)settings.GroupMethod;
            ListSorter_ListBox.SelectedIndex = (int)settings.SortMethod;
        }

        private void DataSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == NotifyCollectionChangedAction.Remove || e.Action == NotifyCollectionChangedAction.Reset) && !vm.DataSource.Any() && Frame.CanGoBack)
                Frame.GoBack();
        }

        private void ReverseItems_Button_Click(object sender, RoutedEventArgs e)
        {
            vm.ReverseItems();
            settings.IsReverse = !settings.IsReverse;
        }

        private void ListSorter_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var id = ListSorter_ListBox.SelectedIndex;
            bool isEqual = (int) settings.SortMethod == id;

            settings.SortMethod = (MusicSorterMembers) id;
            var sorter = vm.SorterMembers[id];

            if (isEqual)
                vm.AutoSort();
            else
                vm.SortItems(sorter);
        }

        private void Grouper_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.GroupMethod = (MusicGrouperMembers) Grouper_ListBox.SelectedIndex;
        }

        private void Main_ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (MusicFileDynamic item in e.AddedItems)
                item.IsSelected = true;
            foreach (MusicFileDynamic item in e.RemovedItems)
                item.IsSelected = false;
        }
        
        private async void MusicFileItemTemplate_OnPlayRequested(object sender, RoutedEventArgs e)
        {
            var theTemplate = sender as MusicFileItemTemplate;
            if (theTemplate is null)
                return;
            
            await MusicPusher.Push(theTemplate.Source.Original);
        }

        private async void PlayAll_Button_OnClick(object sender, RoutedEventArgs e)
        {
            await MusicPusher.Push(vm.GetAllMusic().Select(f => f.Original));
        }

        private async void PlayGroup_Button_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var theButton = sender as Button;
            if (theButton is null)
                return;

            e.Handled = true;
            theButton.IsEnabled = false;
            var source = theButton.DataContext as MusicFileGroupDynamic;
            await MusicPusher.Push(source.Items.Select(f => f.Original));
            theButton.IsEnabled = true;
        }

        private async void DataServer_DataAdded(object sender, IEnumerable<MusicFileDTO> e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate
            {
                vm.AutoSort();
            });
        }

        private void GroupOption_Add_Button_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private async void AddToNowPlaying_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            var theMenu = sender as MenuFlyoutItem;
            if (theMenu == null)
                return;

            theMenu.IsEnabled = false;

            var source = theMenu.DataContext as MusicFileGroupDynamic;
            if (source == null)
                return;
            
            await MusicPusher.Append(source.Items.Select(g => g.Original));
            theMenu.IsEnabled = true;
        }

        private void AddToFavorites_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            var theMenu = sender as MenuFlyoutItem;
            if (theMenu == null)
                return;

            var source = theMenu.DataContext as MusicFileGroupDynamic;
            if (source == null)
                return;

            FavoriteAdditionNotification.RequestFavoriteAddition(source.Items.Select(g => g.Original));
        }

        private void SwitchMultipleSelection_FloatingActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            Main_ListView.SelectionMode = ListViewSelectionMode.Multiple;
            ListOption_CommandBar.Visibility = Visibility.Visible;
            SwitchSingleSelection_FloatingActionButton.Visibility = Visibility.Visible;
            SwitchMultipleSelection_FloatingActionButton.Visibility = Visibility.Collapsed;
        }

        private void SwitchSingleSelection_FloatingActionButton_OnClick(object sender, RoutedEventArgs e)
        {
            Main_ListView.SelectionMode = ListViewSelectionMode.Single;
            ListOption_CommandBar.Visibility = Visibility.Collapsed;
            SwitchSingleSelection_FloatingActionButton.Visibility = Visibility.Collapsed;
            SwitchMultipleSelection_FloatingActionButton.Visibility = Visibility.Visible;
        }

        private void SelectAll_AppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            Main_ListView.SelectAll();
        }

        private async void PlaySelectedItems_AppBarButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Main_ListView.SelectedItems.Any())
                return;
            
            await MusicPusher.Push(Main_ListView.SelectedItems.Cast<MusicFileDynamic>().Select(d => d.Original));
        }

        private async void AddSelectedItemsToNowPlaying_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Main_ListView.SelectedItems.Any())
                return;

            await MusicPusher.Append(Main_ListView.SelectedItems.Cast<MusicFileDynamic>().Select(d => d.Original));
        }

        private void AddSelectedItemsToFavorites_MenuFlyoutItem_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Main_ListView.SelectedItems.Any())
                return;

            FavoriteAdditionNotification.RequestFavoriteAddition(Main_ListView.SelectedItems.Cast<MusicFileDynamic>().Select(d => d.Original));
        }

        private void Search_AutoSuggestBox_OnTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var trigger = SearchTrigger_ComboBox.SelectedItem as SearchTriggerUI<MusicFileDTO>;
            if (trigger is null)
                return;

            sender.ItemsSource = vm.AllItems.Where(m => trigger.Selector.Invoke(m).ToLower().Contains(sender.Text.ToLower()));
        }

        private void Search_AutoSuggestBox_OnSuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            var source = args.SelectedItem as MusicFileDTO;
            if (source is null)
                return;

            sender.Text = source.Title;

            var allItems = vm.DataSource.Select(g => g.Items).Aggregate((l, f) =>
            {
                var result = new List<MusicFileDynamic>();
                result.AddRange(l);
                result.AddRange(f);
                return new ObservableCollection<MusicFileDynamic>(result);
            });

            var item = allItems.FirstOrDefault(d => d.Original.Equals(source));
            if (item is null)
                return;

            Main_ListView.SelectedItem = item;
            Main_ListView.ScrollIntoView(item, ScrollIntoViewAlignment.Leading);
        }
    }
}
