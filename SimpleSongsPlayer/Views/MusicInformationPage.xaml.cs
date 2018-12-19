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
using SimpleSongsPlayer.Models.DTO.Lyric;
using SimpleSongsPlayer.ViewModels;
using SimpleSongsPlayer.ViewModels.Attributes;
using SimpleSongsPlayer.ViewModels.Events;
using SimpleSongsPlayer.Views.Controllers;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    [PageTitle("MusicInfoPage")]
    public sealed partial class MusicInformationPage : Page
    {
        private MusicInfoViewModel vm;
        public MusicInformationPage()
        {
            this.InitializeComponent();
            vm = DataContext as MusicInfoViewModel;
            CustomMediaPlayerElement.PositionChanged += CustomMediaPlayerElement_PositionChanged;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            await vm.Init();
        }

        private void CustomMediaPlayerElement_PositionChanged(CustomMediaPlayerElement sender, PlayerPositionChangeEventArgs args)
        {
            if (args.IsUser)
                My_ScrollLyricsPreviewControl.Reposition(args.Position);
            else
                My_ScrollLyricsPreviewControl.RefreshLyric(args.Position);
        }

        private void My_ScrollLyricsPreviewControl_OnItemClick(object sender, ItemClickEventArgs e)
        {
            CustomMediaPlayerElement.SetPosition_Global(((LyricLine) e.ClickedItem).Time);
        }
    }
}
