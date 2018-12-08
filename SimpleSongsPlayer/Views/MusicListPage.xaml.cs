using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.Factories;
using SimpleSongsPlayer.ViewModels.Factories.MusicFilters;
using SimpleSongsPlayer.ViewModels.Factories.MusicGroupers;
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

        public MusicListPage()
        {
            this.InitializeComponent();
            vm = (MusicListViewModel) DataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ObservableCollection<MusicFileDTO> dtos)
                vm.SetUpDataSource(dtos);
            if (e.Parameter is ValueTuple<ObservableCollection<MusicFileDTO>, MusicFilterArgs> fullParameter)
                vm.SetUpDataSource(fullParameter.Item1, fullParameter.Item2);
        }

        private async void MusicFileItemTemplate_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var theTemplate = sender as MusicFileItemTemplate;
            if (theTemplate is null)
                return;
            
            await MusicPusher.Push(theTemplate.Source.Original);
        }

        private void SwitchGrouper_Button_OnClick(object sender, RoutedEventArgs e)
        {
            vm.GroupItems(needClear: true, grouper: new CharacterGrouper());
        }

        private void SwitchSorter_Button_OnClick(object sender, RoutedEventArgs e)
        {
            vm.SortItems((s, c) => s.Original.Album);
        }
        
        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Grouper_ListBox.SelectedIndex)
            {
                case 0:
                    vm.GroupItems(needClear:true, grouper:new SingleGrouper());
                    break;
                case 1:
                    vm.GroupItems(needClear:true, grouper:new CharacterGrouper());
                    break;
            }
        }
    }
}
