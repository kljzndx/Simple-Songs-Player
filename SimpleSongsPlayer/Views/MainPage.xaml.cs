using System;
using System.Collections.Generic;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels;
// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private readonly MainViewModel vm;

        public MainPage()
        {
            this.InitializeComponent();
            vm = this.DataContext as MainViewModel;
        }
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (!(e.Parameter is ValueTuple<List<Song>, List<LyricBlock>>))
                throw new Exception("未传入资源");
            
            ValueTuple<List<Song>, List<LyricBlock>> tuple = (ValueTuple<List<Song>, List<LyricBlock>>) e.Parameter;
            vm.AllSongs = tuple.Item1;
            vm.AllLyricBlocks = tuple.Item2;
        }
        
        private void MainPage_OnLoaded(object sender, RoutedEventArgs e)
        {
        }
    }
}
