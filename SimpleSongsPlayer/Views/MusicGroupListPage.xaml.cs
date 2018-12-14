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
using SimpleSongsPlayer.ViewModels.Arguments;
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

        private List<MusicItemMenuItem<MusicFileDynamic>> itemExtraMenu;
        private readonly List<MusicItemMenuItem<MusicFileGroupDynamic>> groupMenu = new List<MusicItemMenuItem<MusicFileGroupDynamic>>();

        public MusicGroupListPage()
        {
            this.InitializeComponent();
            vm = (MusicGroupListViewModel) DataContext;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is MusicGroupArguments args)
            {
                if (args.ArgsType.HasFlag(MusicGroupArgsType.Menu))
                {
                    groupMenu.AddRange(args.ExtraGroupMenu);
                    itemExtraMenu = args.ExtraItemMenu;
                }

                switch (args.ArgsType & (~MusicGroupArgsType.Menu))
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

        private void Main_GridView_OnItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as MusicFileGroup;
            if (item is null)
                return;

            if (vm.ItemFilter != null)
                if (itemExtraMenu != null)
                    Frame.Navigate(typeof(MusicListPage), new MusicListArguments(vm.Original, new MusicFilterArgs(vm.ItemFilter, item.Name), itemExtraMenu));
                else
                    Frame.Navigate(typeof(MusicListPage), new MusicListArguments(vm.Original, new MusicFilterArgs(vm.ItemFilter, item.Name)));
            else if (itemExtraMenu != null)
                Frame.Navigate(typeof(MusicListPage), new MusicListArguments(item.Items, itemExtraMenu));
            else
                Frame.Navigate(typeof(MusicListPage), new MusicListArguments(item.Items));
        }
    }
}
