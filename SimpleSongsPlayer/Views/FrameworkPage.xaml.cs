using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.Views.SongViews;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class FrameworkPage : Page
    {
        private readonly FrameworkViewModel vm;

        public FrameworkPage()
        {
            this.InitializeComponent();
            vm = this.DataContext as FrameworkViewModel;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is ValueTuple<List<Song>, List<LyricBlock>> tuple)
            {
                vm.AllLyricBlocks = tuple.Item2;
                Main_Frame.Navigate(typeof(AllSongListsPage), tuple.Item1);
            }
            else
                throw new Exception("未传入资源");
        }

    }
}
