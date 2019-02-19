using System;
using System.Collections.Generic;
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
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Getters;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views.MusicInfo
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicInformationSmallPage : MusicInformationPageBase
    {
        public MusicInformationSmallPage()
        {
            this.InitializeComponent();
            base.Init((MusicInfoViewModel) this.DataContext);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Ms_AdControl.AdUnitId != PrivateKeyGetter.AdSmallKey)
                Ms_AdControl.AdUnitId = PrivateKeyGetter.AdSmallKey;

            if (Root_Pivot.SelectedIndex == 1 && Settings.IsShowAds)
                Ms_AdControl.Resume();

            if (!Settings.IsShowAds)
                Ms_AdControl.Suspend();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (Settings.IsShowAds)
                Ms_AdControl.Suspend();
        }

        private void Root_Pivot_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Root_Pivot.SelectedIndex == 1 && Settings.IsShowAds)
                Ms_AdControl.Resume();
            else if (Settings.IsShowAds)
                Ms_AdControl.Suspend();
        }
    }
}
