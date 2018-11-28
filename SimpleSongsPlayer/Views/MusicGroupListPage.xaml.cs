using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.Models.DTO;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Factories;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicGroupListPage : Page
    {
        private MusicGroupListViewModel vm;

        public MusicGroupListPage()
        {
            this.InitializeComponent();
            vm = (MusicGroupListViewModel) DataContext;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is ObservableCollection<MusicFileGroup> list)
                vm.SetUp(list);
            if (e.Parameter is ValueTuple<ObservableCollection<MusicFileDTO>, MusicGrouperArgs> tuple)
                await vm.SetUp(tuple.Item1, tuple.Item2);
        }
    }
}
