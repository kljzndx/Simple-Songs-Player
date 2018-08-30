using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.ViewModels.SongViewModels;

namespace SimpleSongsPlayer.Views.SongViews
{
    public class SongViewsPageBase : Page
    {
        private readonly SongViewModelBase vmb;

        public SongViewsPageBase(SongViewModelBase vm)
        {
            vmb = vm;
        }

        protected T GetViewModel<T>() where T : SongViewModelBase
        {
            if (vmb.GetType() != typeof(T))
                throw new Exception("类型错误");

            return (T) vmb;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is List<Song> allSongs)
                vmb.RefreshData(allSongs);
            else
                throw new Exception("未收到歌曲数据");
        }
    }
}