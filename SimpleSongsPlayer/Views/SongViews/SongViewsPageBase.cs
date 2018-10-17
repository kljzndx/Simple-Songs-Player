using System;
using System.Collections.Generic;
using System.Linq;
using Windows.ApplicationModel.Resources;
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
        private readonly MenuFlyout songItemMenu;
        
        protected Song SongCache;

        protected SongViewsPageBase(SongViewModelBase vm)
        {
            vmb = vm;
            songItemMenu = new MenuFlyout();
            songItemMenu.Closed += (s, e) => SongCache = null;

            Resources = new ResourceDictionary();
            Resources.Add("SongItemMenu", songItemMenu);
            
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected T GetViewModel<T>() where T : SongViewModelBase
        {
            if (vmb.GetType() != typeof(T))
                throw new Exception("提供的vm类型 与 在构造函数里提供的vm的类型 不一致");

            return (T) vmb;
        }

        protected virtual void MenuInit(MenuFlyout menuFlyout, ResourceLoader stringResource)
        {
            MenuFlyoutItem addToPlayListMenu = new MenuFlyoutItem();
            addToPlayListMenu.Text = stringResource.GetString("AddToPlayList");
            addToPlayListMenu.Click += AddToPlayListMenu_Click;

            menuFlyout.Items.Add(addToPlayListMenu);
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is List<Song> allSongs)
                await vmb.RefreshData(allSongs);
            else
                throw new Exception("未收到歌曲数据");
            
            if (!songItemMenu.Items.Any())
                MenuInit(songItemMenu, ResourceLoader.GetForCurrentView("SongItemMenu"));
        }

        protected void Songs_ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var sg in vmb.SongGroups)
                foreach (var item in sg.Items)
                    item.IsSelected = false;

            var song = e.AddedItems.FirstOrDefault() as Song;
            if (song is null)
                return;

            song.IsSelected = true;
        }

        public void Songs_ListView_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            FrameworkElement args = e.OriginalSource as FrameworkElement;
            if (args.DataContext is Song theSong)
                vmb.Push(theSong);
        }

        protected void PlayItem_Button_Tapped(SongItemTemplate sender, EventArgs args)
        {
            vmb.Push(sender.Source);
        }
        
        protected void SongItemTemplate_MenuOpening(SongItemTemplate sender, EventArgs args)
        {
            SongCache = sender.Source;
        }

        private void AddToPlayListMenu_Click(object sender, RoutedEventArgs e)
        {
            if (SongCache is null)
                return;

            vmb.Append(SongCache);
        }

        protected void PlayGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as FrameworkElement;
            var group = btn?.DataContext as SongsGroup;
            if (group != null)
                vmb.Push(group.Items);
        }

        protected void AddGroup_Button_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;
            var btn = sender as FrameworkElement;
            var group = btn?.DataContext as SongsGroup;
            if (group != null)
                vmb.Append(group.Items);
        }
    }
}