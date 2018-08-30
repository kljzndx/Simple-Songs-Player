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
        
        private void Main_Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (Main_Pivot.SelectedIndex)
            {
                case 0:
                    if (AllSongs_Frame.SourcePageType != typeof(SongsViewPage))
                        AllSongs_Frame.Navigate(typeof(SongsViewPage), vm.AllSongs);
                    break;
                case 1:
                    if (AllSongArtists_Frame.SourcePageType != typeof(SongArtistsViewPage))
                        AllSongArtists_Frame.Navigate(typeof(SongArtistsViewPage), vm.AllSongs);
                    break;
                case 2:
                    if (AllSongAlbums_Frame.SourcePageType != typeof(SongAlbumsViewPage))
                        AllSongAlbums_Frame.Navigate(typeof(SongAlbumsViewPage), vm.AllSongs);
                    break;
                default:
                    throw new Exception("未找到对应处理器");
            }
        }
    }
}
