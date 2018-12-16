using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    [PageTitle("MusicListPage", "Title")]
    public sealed partial class MusicListPage : Page
    {
        private MusicListViewModel vm;
        private readonly MusicViewSettingProperties settings = MusicViewSettingProperties.Current;
        private readonly List<MusicItemMenuItem<MusicFileDynamic>> musicItemMenuList;

        public MusicListPage()
        {
            musicItemMenuList = new List<MusicItemMenuItem<MusicFileDynamic>>();
            Resources["MusicItemMenuList"] = musicItemMenuList;

            musicItemMenuList.Add(new MusicItemMenuItem<MusicFileDynamic>("MusicListPage", "MoreMenu_PlayNext", s => MusicPusher.PushToNext(s.Original)));
            musicItemMenuList.Add(new MusicItemMenuItem<MusicFileDynamic>("MusicListPage", "MoreMenu_Favorite", async f => FavoriteAdditionNotification.RequestFavoriteAddition(new[] {f.Original})));

            this.InitializeComponent();
            vm = (MusicListViewModel) DataContext;

            Grouper_ListBox.SelectedIndex = (int) settings.GroupMethod;
            ListSorter_ListBox.SelectedIndex = (int) settings.SortMethod;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MusicListArguments args)
            {
                if (args.ArgsType.HasFlag(MusicListArgsType.Menu))
                    musicItemMenuList.AddRange(args.ExtraMenu);

                switch (args.ArgsType & (~MusicListArgsType.Menu))
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

        private void ListSortSelection_SplitButton_OnLeftButton_Click(object sender, RoutedEventArgs e)
        {
            settings.IsReverse = !settings.IsReverse;
        }

        private void ListSorter_ListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            settings.SortMethod = (MusicSorterMembers) ListSorter_ListBox.SelectedIndex;
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
    }
}
