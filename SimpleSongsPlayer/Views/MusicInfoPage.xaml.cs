using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;

using HappyStudio.Subtitle.Control.Interface.Models;

using SimpleSongsPlayer.ViewModels;

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

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace SimpleSongsPlayer.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MusicInfoPage : Page
    {
        private MusicInfoViewModel _vm;

        public MusicInfoPage()
        {
            this.InitializeComponent();
            _vm = Ioc.Default.GetRequiredService<MusicInfoViewModel>();

            WeakReferenceMessenger.Default.Register<MusicInfoPage, string, string>(this, "MediaPlayer", (page, mes) =>
            {
                string[] split = mes.Split(':');
                string key = split[0];
                string value = split[1];

                var time = TimeSpan.FromMinutes(double.Parse(value));
                if (key == "PositionChangedBySystem")
                    page.MyScrollSubtitlePreview.Refresh(time);
                if (key == "PositionChangedByUser")
                    page.MyScrollSubtitlePreview.Reposition(time);
            });
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _vm.AutoLoad();
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.GoBack();
        }

        private void MyScrollSubtitlePreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SubtitleLineUi;
            if (item == null)
                return;

            WeakReferenceMessenger.Default.Send($"RequestChangePosition:{item.StartTime.TotalMinutes}", "MediaPlayer");
        }
    }
}
