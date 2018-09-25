using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using SimpleSongsPlayer.DataModel;
using SimpleSongsPlayer.Models;
using SimpleSongsPlayer.ViewModels.SongViewModels;
using SimpleSongsPlayer.Views.ItemTemplates;

namespace SimpleSongsPlayer.Views.SongViews
{
    public abstract class SongViewsPageBase : Page
    {
        private readonly SongViewModelBase vmb;

        protected SongViewsPageBase(SongViewModelBase vm)
        {
            vmb = vm;
        }

        protected T GetViewModel<T>() where T : SongViewModelBase
        {
            if (vmb.GetType() != typeof(T))
                throw new Exception("提供的vm类型 与 在构造函数里提供的vm的类型 不一致");

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

        protected void Songs_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var sg in vmb.SongGroups)
                foreach (var item in sg.Items)
                    item.IsSelected = false;

            var song = e.AddedItems.First() as Song;
            song.IsSelected = true;
        }

        protected void PlayItem_Button_Tapped(SongItemTemplate sender, EventArgs args)
        {
            vmb.Push(sender.Source);
        }

        protected void AddItem_Button_Tapped(SongItemTemplate sender, EventArgs args)
        {
            vmb.Append(sender.Source);
        }

        protected void PlayGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as FrameworkElement;
            if (btn is null)
                return;
            var group = btn.DataContext as SongsGroup;

            vmb.Push(group.Items);
        }

        protected void AddGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as FrameworkElement;
            if (btn is null)
                return;
            var group = btn.DataContext as SongsGroup;

            vmb.Append(group.Items);
        }
    }
}