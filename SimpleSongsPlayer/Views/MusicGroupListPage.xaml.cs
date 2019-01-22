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
using SimpleSongsPlayer.Service;
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

        private List<MusicItemMenuItem<MusicFileDynamic>> itemExtraMenu;
        private readonly List<MusicItemMenuItem<MusicFileGroupDynamic>> groupMenu = new List<MusicItemMenuItem<MusicFileGroupDynamic>>();

        public MusicGroupListPage()
        {
            groupMenu.Add(new MusicItemMenuItem<MusicFileGroupDynamic>("MusicGroupPage", "MoreMenu_Favorite", async g => FavoriteAdditionNotification.RequestFavoriteAddition(g.Items.Select(f => f.Original))));
            this.InitializeComponent();
            vm = (MusicGroupListViewModel) DataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MusicGroupArguments args)
            {
                if (args.ArgsType.HasFlag(MusicGroupArgsType.GroupMenu))
                    groupMenu.AddRange(args.ExtraGroupMenu);
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
            Sorter_ListView.SelectedIndex = (int)settings.SortMethod;
        }

        private void Main_GridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicFileGroup;
            if (item is null)
                return;

            if (vm.ItemFilter != null)
                if (itemExtraMenu != null)
                    Frame.Navigate(typeof(MusicListPage), new MusicListArguments(vm.Original, MusicFileDataServer.Current, new MusicFilterArgs(vm.ItemFilter, item.Name), itemExtraMenu, pageTitle: item.Name));
                else
                    Frame.Navigate(typeof(MusicListPage), new MusicListArguments(vm.Original, MusicFileDataServer.Current, new MusicFilterArgs(vm.ItemFilter, item.Name), pageTitle: item.Name));
            else if (itemExtraMenu != null)
                Frame.Navigate(typeof(MusicListPage), new MusicListArguments(item.Items, itemExtraMenu, item.Name));
            else
                Frame.Navigate(typeof(MusicListPage), new MusicListArguments(item.Items, item.Name));
        }

        private async void PlayAll_Button_OnClick(object sender, RoutedEventArgs e)
        {
            PlayAll_Button.IsEnabled = false;

            this.LogByObject("正在提取出所有的音乐");
            var data = vm.DataSource.Select(g => g.Items).Aggregate((l, r) =>
            {
                var d = new ObservableCollection<MusicFileDTO>(l);
                foreach (var musicFileDto in r)
                    d.Add(musicFileDto);
                return d;
            });

            if (data.Any())
                await MusicPusher.Push(data);

            PlayAll_Button.IsEnabled = true;
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

        private void Sorter_ListView_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bool isEqual = (int) settings.SortMethod == Sorter_ListView.SelectedIndex;
            settings.SortMethod =
                (MusicGroupSorterMembers) Sorter_ListView.SelectedIndex;

            if (isEqual)
                vm.AutoSort();
            else
                vm.Sort(e.AddedItems.Cast<MusicSorterUi<MusicFileGroup>>().First());
        }

        private void Sort_SplitButton_OnLeftButton_Click(object sender, RoutedEventArgs e)
        {
            vm.Reverse();
            settings.IsReverse = !settings.IsReverse;
        }
    }
}
