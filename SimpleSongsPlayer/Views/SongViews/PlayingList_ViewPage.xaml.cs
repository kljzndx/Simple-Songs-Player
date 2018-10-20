﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Resources;
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
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.SongViewModels;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.SongViews
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class PlayingList_ViewPage : SongViewsPageBase
    {
        private PlayingListViewModel vm;

        public PlayingList_ViewPage(): base(((ViewModelLocator)Application.Current.Resources["Locator"]).SongsLocator.PlayingList)
        {
            this.InitializeComponent();
            vm = base.GetViewModel<PlayingListViewModel>();
        }

        protected override void MenuInit(MenuFlyout menuFlyout, ResourceLoader stringResource)
        {
            base.MenuInit(menuFlyout, stringResource);

            MenuFlyoutItem remove = new MenuFlyoutItem();
            remove.Text = stringResource.GetString("Remove");
            remove.Click += (s, e) => SongCache?.RequestRemove();

            menuFlyout.Items.Add(remove);
        }

        private  void Delete_Button_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            var theButton = sender as Button;

            var theGroup = theButton?.DataContext as SongsGroup;
            if (theGroup is null)
                return;

            vm.SongGroups.Remove(theGroup);
        }
    }
}
