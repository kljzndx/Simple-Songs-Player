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
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

//https://go.microsoft.com/fwlink/?LinkId=234236 上介绍了“用户控件”项模板

namespace SimpleSongsPlayer.Views.Controllers
{
    public sealed partial class MusicInfoDisplay : UserControl
    {
        private MusicInfoViewModel _vm;

        public MusicInfoDisplay()
        {
            this.InitializeComponent();
            _vm = Ioc.Default.GetRequiredService<MusicInfoViewModel>();

            WeakReferenceMessenger.Default.Register<MusicInfoDisplay, string, string>(this, "MediaPlayer", (ctor, mes) =>
            {
                string[] split = mes.Split(':');
                string key = split[0];
                string value = split[1];

                var time = TimeSpan.FromMinutes(double.Parse(value));
                if (key == "PositionChangedBySystem")
                    ctor.MyScrollSubtitlePreview.Refresh(time);
                if (key == "PositionChangedByUser")
                    ctor.MyScrollSubtitlePreview.Reposition(time);
            });

            WeakReferenceMessenger.Default.Register<MusicInfoDisplay, string, string>(this, nameof(MusicInfoDisplay), (ctor, mes) =>
            {
                if (mes == "RequestReposition")
                {
                    var player = Ioc.Default.GetRequiredService<MediaPlayer>();
                    MyScrollSubtitlePreview.Reposition(player.PlaybackSession.Position);
                }
            });
        }

        private void MyScrollSubtitlePreview_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as SubtitleLineUi;
            if (item == null)
                return;

            WeakReferenceMessenger.Default.Send($"RequestChangePosition:{item.StartTime.TotalMinutes}", "MediaPlayer");
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            WeakReferenceMessenger.Default.Send("RequestReposition", nameof(MusicInfoDisplay));
        }
    }
}
